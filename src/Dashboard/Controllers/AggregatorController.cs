using Dashboard.Models;
using Dashboard.Models.Webhooks;
using Dashboard.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
        public async Task<ActionResult<Sensor[]>> AggregateDataAsync()
        {
            var sensors = this._sensorService.GetSensors();
            var startDate = DateTime.Today.AddDays(-1);

            foreach (var sensor in sensors)
            {
                for (int i = 0; i < 90; i++)
                {
                    var processDate = DateOnly.FromDateTime(startDate.AddDays(-i));
                    var succesful = await this.AggregateDateAsync(sensor, processDate);
                    if (!succesful)
                    {
                        this._logger.LogError($"{nameof(AggregateDataAsync)} - {sensor.DeviceId} {processDate}");
                        continue;
                    }

                    this._logger.LogInformation($"{nameof(AggregateDataAsync)} - {sensor.DeviceId} {processDate}");
                }
            }

            return StatusCode(StatusCodes.Status200OK, sensors);
        }

        private async Task<bool> AggregateDateAsync(Sensor sensor, DateOnly date)
        {
            var fileInfos = await this._objectStorageService.GetFileInfosAsync($"{sensor.DeviceId}-{date:yyyy-MM-dd}");

            var sensorDataKeys = new List<string>();

            foreach (var fileInfo in fileInfos.OrderBy(o => o.Key))
            {
                if (fileInfo.Key.Length < 42)
                {
                    continue;
                }

                sensorDataKeys.Add(fileInfo.Key);
            }

            if (!sensorDataKeys.Any())
            {
                return false;
            }

            var getFileTasks = new List<Task<UplinkMessageWebhook?>>();
            foreach (var sensorDataKey in sensorDataKeys)
            {
                getFileTasks.Add(this.GetUplinkMessageWebhookAsync(sensorDataKey));
            }
            var results = await Task.WhenAll(getFileTasks);

            var uplinkMessageWebhooks = new List<UplinkMessageWebhook>();
            uplinkMessageWebhooks.AddRange(results.Where(o => o != null).Select(o => o!));

            var records = uplinkMessageWebhooks.Select(o => new SensorDetailRecord
            {
                PM1 = o.UplinkMessage?.DecodedPayload.Decoded.PM1 ?? 0,
                PM2_5 = o.UplinkMessage?.DecodedPayload.Decoded.PM2_5 ?? 0,
                PM4 = o.UplinkMessage?.DecodedPayload.Decoded.PM4 ?? 0,
                PM10 = o.UplinkMessage?.DecodedPayload.Decoded.PM10 ?? 0,
                ParticlesPerCubicCentimeterPM0_5 = o.UplinkMessage?.DecodedPayload.Decoded.ParticlesPerCubicCentimeterPM0_5 ?? 0,
                ParticlesPerCubicCentimeterPM1 = o.UplinkMessage?.DecodedPayload.Decoded.ParticlesPerCubicCentimeterPM1 ?? 0,
                ParticlesPerCubicCentimeterPM2_5 = o.UplinkMessage?.DecodedPayload.Decoded.ParticlesPerCubicCentimeterPM2_5 ?? 0,
                ParticlesPerCubicCentimeterPM4 = o.UplinkMessage?.DecodedPayload.Decoded.ParticlesPerCubicCentimeterPM4 ?? 0,
                ParticlesPerCubicCentimeterPM10 = o.UplinkMessage?.DecodedPayload.Decoded.ParticlesPerCubicCentimeterPM10 ?? 0,
                Humidity = o.UplinkMessage?.DecodedPayload.Decoded.Humidity ?? 0,
                Temperature = o.UplinkMessage?.DecodedPayload.Decoded.Temperature ?? 0,
                Timestamp = DateTime.TryParse(o?.UplinkMessage?.ReceivedAt, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime receivedAt) ? receivedAt : DateTime.MinValue
            }).ToArray();

            if (records == null)
            {
                return false;
            }

            var sensorDayData = new SensorDayData
            {
                DeviceId = sensor.DeviceId,
                Date = date,
                City = sensor.City,
                Name = sensor.Name,
                District = sensor.District,
                Average = new SensorAverage
                {
                    PM1 = Math.Round(records.Average(record => record.PM1), 2, MidpointRounding.AwayFromZero),
                    PM2_5 = Math.Round(records.Average(record => record.PM2_5), 2, MidpointRounding.AwayFromZero),
                    PM4 = Math.Round(records.Average(record => record.PM4), 2, MidpointRounding.AwayFromZero),
                    PM10 = Math.Round(records.Average(record => record.PM10), 2, MidpointRounding.AwayFromZero),
                    Humidity = Math.Round(records.Average(record => record.Humidity), 2, MidpointRounding.AwayFromZero),
                    Temperature = Math.Round(records.Average(record => record.Temperature), 2, MidpointRounding.AwayFromZero)
                },
                SensorDetailRecords = records,
            };

            using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(memoryStream, sensorDayData);
            var test = JsonSerializer.Serialize(sensorDayData);

            var key = $"{sensor.DeviceId}-{date:yyyy-MM-dd}.json";
            await this._objectStorageService.UploadFileAsync(key, memoryStream);

            await this._objectStorageService.DeleteFilesAsync([.. sensorDataKeys]);

            return true;
        }

        private async Task<UplinkMessageWebhook?> GetUplinkMessageWebhookAsync(string key)
        {
            var fileData = await this._objectStorageService.GetFileAsync(key);
            //this._logger.LogInformation($"{nameof(GetUplinkMessageWebhookAsync)} - Get data {key} {fileData.Length}");

            return JsonSerializer.Deserialize<UplinkMessageWebhook>(fileData, this._jsonSerializerOptions);
        }
    }
}
