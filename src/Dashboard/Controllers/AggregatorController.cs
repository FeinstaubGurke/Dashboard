using Dashboard.Models;
using Dashboard.Models.Webhooks;
using Dashboard.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using ThirdParty.Json.LitJson;

namespace Dashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AggregatorController : ControllerBase
    {
        private readonly ILogger<AggregatorController> _logger;
        private readonly SensorService _sensorService;
        private readonly IObjectStorageService _objectStorageService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AggregatorController(
            ILogger<AggregatorController> logger,
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

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<Sensor[]>> SensorsAsync()
        {
            var sensors = this._sensorService.GetSensors();

            foreach (var sensor in sensors)
            {
                var x = await this.AggregateDateAsync(sensor.DeviceId, new DateOnly(2024, 12, 22));


                //var fileInfos = await this._objectStorageService.GetFileInfosAsync(sensor.DeviceId);



                //foreach (var fileInfo in fileInfos.OrderBy(o => o.Key))
                //{
                //    var fileData = await this._objectStorageService.GetFileAsync(fileInfo.Key);
                //    this._logger.LogInformation($"Get data {fileData.Length}");
                //}
            }

            return StatusCode(StatusCodes.Status200OK, sensors);
        }

        private async Task<bool> AggregateDateAsync(string sensorDeviceId, DateOnly date)
        {
            var fileInfos = await this._objectStorageService.GetFileInfosAsync($"{sensorDeviceId}-{date:yyyy-MM-dd}");

            var getFileTasks = new List<Task<UplinkMessageWebhook?>>();

            foreach (var fileInfo in fileInfos.OrderBy(o => o.Key))
            {
                getFileTasks.Add(this.GetUplinkMessageWebhookAsync(fileInfo.Key));
            }

            var results = await Task.WhenAll(getFileTasks);

            var uplinkMessageWebhooks = new List<UplinkMessageWebhook>();
            uplinkMessageWebhooks.AddRange(results.Where(o => o != null).Select(o => o!));

            return true;
        }

        private async Task<UplinkMessageWebhook?> GetUplinkMessageWebhookAsync(string key)
        {
            var fileData = await this._objectStorageService.GetFileAsync(key);
            this._logger.LogInformation($"{nameof(GetUplinkMessageWebhookAsync)} - Get data {key} {fileData.Length}");

            return JsonSerializer.Deserialize<UplinkMessageWebhook>(fileData, this._jsonSerializerOptions);
        }
    }
}
