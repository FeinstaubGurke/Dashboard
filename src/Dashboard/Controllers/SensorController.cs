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
        private readonly SensorService _sensorService;

        public SensorController(
            ILogger<SensorController> logger,
            SensorService sensorService)
        {
            this._logger = logger;
            this._sensorService = sensorService;
        }

        [HttpGet]
        [Route("")]
        public ActionResult<Sensor[]> Sensors()
        {
            var sensors = this._sensorService.GetSensors();

            return StatusCode(StatusCodes.Status200OK, sensors);
        }
    }
}
