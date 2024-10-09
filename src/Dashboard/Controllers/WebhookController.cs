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

        [HttpGet]
        public ActionResult Get()
        {
            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
