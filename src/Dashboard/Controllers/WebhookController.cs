using Dashboard.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(ILogger<WebhookController> logger)
        {
            this._logger = logger;
        }

        [HttpPost]
        [Route("JoinAccept")]
        public ActionResult JoinAccept(JoinAcceptWebhook joinAcceptWebhook)
        {
            this._logger.LogInformation("JoinAccept");

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPost]
        [Route("UplinkMessage")]
        public ActionResult UplinkMessage(UplinkMessageWebhook uplinkMessageWebhook)
        {
            this._logger.LogInformation("UplinkMessage");

            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
