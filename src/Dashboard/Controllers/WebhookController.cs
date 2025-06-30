using Dashboard.Models.Webhooks;
using Dashboard.Models.Webhooks.SensorData;
using Dashboard.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Dashboard.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly SensorService _sensorService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly IObjectStorageService _objectStorageService;

        public WebhookController(
            ILogger<WebhookController> logger,
            SensorService sensorService,
            IObjectStorageService objectStorageService)
        {
            this._logger = logger;
            this._sensorService = sensorService;
            this._objectStorageService = objectStorageService;

            this._jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
        }

        private bool CheckApiKey()
        {
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                if (authorizationHeader.Count != 1)
                {
                    return false;
                }

                var tempAuthorizationHeader = authorizationHeader[0].AsSpan();

                if (!tempAuthorizationHeader.StartsWith("Bearer "))
                {
                    return false;
                }

                var apiKey = tempAuthorizationHeader.Slice(7);

                // Header gefunden, userAgent ist ein StringValues-Objekt
                //string value = userAgent.ToString(); // oder ggf. userAgent.FirstOrDefault()
                this._logger.LogInformation($"{nameof(CheckApiKey)} - {apiKey}");
                return true;
            }

            return false;
        }

        [HttpPost]
        [Route("JoinAccept")]
        public ActionResult JoinAccept(
            [FromBody] JsonElement requestBody)
        {
            this.CheckApiKey();

            var webhook = JsonSerializer.Deserialize<JoinAcceptWebhook>(requestBody.GetRawText(), this._jsonSerializerOptions);
            if (webhook is null)
            {
                this._logger.LogInformation($"{nameof(JoinAccept)} - Deserialize failure");
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            this._logger.LogInformation($"{nameof(JoinAccept)} - {webhook.EndDeviceIds.DeviceId}");

            this._sensorService.SetTryJoin(webhook.EndDeviceIds.DeviceId);

            return StatusCode(StatusCodes.Status202Accepted);
        }

        [HttpPost]
        [Route("UplinkMessage")]
        public async Task<ActionResult> UplinkMessage(
            [FromBody] JsonElement requestBody,
            CancellationToken cancellationToken = default)
        {
            this.CheckApiKey();

            var webhookBase = JsonSerializer.Deserialize<TheThingsNetworkWebhookBase>(requestBody.GetRawText(), this._jsonSerializerOptions);
            if (webhookBase is null)
            {
                this._logger.LogInformation($"{nameof(UplinkMessage)} - Deserialize failure");
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            this._logger.LogInformation(webhookBase.EndDeviceIds.ApplicationIds.ApplicationId);
            this._logger.LogInformation($"{nameof(ProcessParticulateMatterAsync)} - {webhookBase.EndDeviceIds.DeviceId}");

            if (webhookBase.EndDeviceIds.ApplicationIds.ApplicationId == "feinstaubgurke")
            {
                if (await this.ProcessParticulateMatterAsync(requestBody, cancellationToken))
                {
                    return StatusCode(StatusCodes.Status202Accepted);
                }

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            if (webhookBase.EndDeviceIds.ApplicationIds.ApplicationId == "windgurke")
            {
                this._logger.LogDebug("add logic");
                return StatusCode(StatusCodes.Status202Accepted);
            }

            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        private async Task<bool> ProcessParticulateMatterAsync(
            JsonElement requestBody,
            CancellationToken cancellationToken = default)
        {
            var webhook = JsonSerializer.Deserialize<UplinkMessageWebhook<ParticulateMatterDecoded>>(requestBody.GetRawText(), this._jsonSerializerOptions);
            if (webhook is null)
            {
                this._logger.LogInformation($"{nameof(ProcessParticulateMatterAsync)} - Deserialize failure");
                return false;
            }

            this._sensorService.UpdateSensorData(
                webhook.EndDeviceIds.DeviceId,
                webhook.UplinkMessage.DecodedPayload?.Decoded?.TxReason ?? "",
                webhook.UplinkMessage.DecodedPayload?.Decoded.PM1,
                webhook.UplinkMessage.DecodedPayload?.Decoded.PM2_5);

            using var memoryStream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(memoryStream))
            {
                requestBody.WriteTo(writer);
            }

            await this._objectStorageService.UploadFileAsync($"{webhook.EndDeviceIds.DeviceId}-{DateTime.Now:yyyy-MM-dd_HH_mm}.json", memoryStream);

            return true;
        }
    }
}
