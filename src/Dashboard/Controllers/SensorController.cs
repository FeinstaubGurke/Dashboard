using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SensorController : ControllerBase
    {
        private readonly ILogger<SensorController> _logger;
        private readonly SensorStatusService _sensorStatusService;

        public SensorController(
            ILogger<SensorController> logger,
            SensorStatusService sensorStatusService)
        {
            this._logger = logger;
            this._sensorStatusService = sensorStatusService;
        }

        [HttpGet]
        [Route("")]
        public ActionResult Sensors()
        {
            return StatusCode(StatusCodes.Status200OK, string.Join(",", this._sensorStatusService.GetSensors()));
        }
    }
}
