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
        private readonly SensorService _sensorService;

        public WebhookController(
            ILogger<WebhookController> logger,
            SensorService sensorService)
        {
            this._logger = logger;
            this._sensorService = sensorService;
        }

        [HttpPost]
        [Route("JoinAccept")]
        public ActionResult JoinAccept(JoinAcceptWebhook webhook)
        {
            this._logger.LogInformation($"JoinAccept - {webhook.EndDeviceIds.DeviceId}");

            this._sensorService.SetTryJoin(webhook.EndDeviceIds.DeviceId);

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPost]
        [Route("UplinkMessage")]
        public ActionResult UplinkMessage(UplinkMessageWebhook webhook)
        {
            this._logger.LogInformation($"UplinkMessage - {webhook.EndDeviceIds.DeviceId} Sensormodus:{webhook.UplinkMessage.DecodedPayload.Decoded.Sensormodus} TxReason:{webhook.UplinkMessage.DecodedPayload.Decoded.TxReason}");

            this._sensorService.UpdateStatus(webhook.EndDeviceIds.DeviceId, webhook.UplinkMessage.DecodedPayload.Decoded.TxReason);

            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
