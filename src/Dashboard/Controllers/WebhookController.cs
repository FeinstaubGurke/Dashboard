using Dashboard.Models.Webhooks;
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

        public WebhookController(
            ILogger<WebhookController> logger,
            SensorService sensorService)
        {
            this._logger = logger;
            this._sensorService = sensorService;

            this._jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
        }

        [HttpPost]
        [Route("JoinAccept")]
        public ActionResult JoinAccept([FromBody] JsonElement body)
        {
            var webhook = JsonSerializer.Deserialize<JoinAcceptWebhook>(body.GetRawText(), this._jsonSerializerOptions);
            if (webhook == null)
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
        public ActionResult UplinkMessage([FromBody] JsonElement body)
        {
            var webhook = JsonSerializer.Deserialize<UplinkMessageWebhook>(body.GetRawText(), this._jsonSerializerOptions);
            if (webhook == null)
            {
                this._logger.LogInformation($"{nameof(UplinkMessage)} - Deserialize failure");
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            this._logger.LogInformation($"{nameof(UplinkMessage)} - {webhook.EndDeviceIds.DeviceId} Sensormodus:{webhook.UplinkMessage.DecodedPayload.Decoded.Sensormodus} TxReason:{webhook.UplinkMessage.DecodedPayload.Decoded.TxReason}");

            this._sensorService.UpdateStatus(
                webhook.EndDeviceIds.DeviceId,
                webhook.UplinkMessage.DecodedPayload.Decoded.TxReason,
                webhook.UplinkMessage.DecodedPayload.Decoded.PM1,
                webhook.UplinkMessage.DecodedPayload.Decoded.PM2_5);

            return StatusCode(StatusCodes.Status202Accepted);
        }
    }
}
