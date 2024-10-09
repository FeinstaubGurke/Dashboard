using Dashboard.ActionFilters;
using Dashboard.Models.Webhooks;
using Dashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly SensorStatusService _sensorStatusService;

        public WebhookController(
            ILogger<WebhookController> logger,
            SensorStatusService sensorStatusService)
        {
            this._logger = logger;
            this._sensorStatusService = sensorStatusService;
        }

        [HttpPost]
        [Route("JoinAccept")]
        [ServiceFilter(typeof(CustomJsonNamingPolicyFilter))]
        public ActionResult JoinAccept(JoinAcceptWebhook webhook)
        {
            this._logger.LogInformation($"JoinAccept - {webhook.EndDeviceIds.DeviceId}");

            this._sensorStatusService.SetTryJoin(webhook.EndDeviceIds.DeviceId);

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPost]
        [Route("UplinkMessage")]
        [ServiceFilter(typeof(CustomJsonNamingPolicyFilter))]
        public ActionResult UplinkMessage(UplinkMessageWebhook webhook)
        {
            this._logger.LogInformation($"UplinkMessage - {webhook.EndDeviceIds.DeviceId} Sensormodus:{webhook.UplinkMessage.DecodedPayload.Decoded.Sensormodus} TxReason:{webhook.UplinkMessage.DecodedPayload.Decoded.TxReason}");

            this._sensorStatusService.UpdateStatus(webhook.EndDeviceIds.DeviceId, webhook.UplinkMessage.DecodedPayload.Decoded.TxReason);

            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
