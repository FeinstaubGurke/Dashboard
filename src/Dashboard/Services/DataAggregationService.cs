using Dashboard.Models;
using Dashboard.Models.Webhooks;
using System.Text.Json;

namespace Dashboard.Services
{
    public class DataAggregationService : IDataAggregationService
    {
        private readonly ILogger<DataAggregationService> _logger;
        private readonly SensorService _sensorService;
        private readonly IObjectStorageService _objectStorageService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public DataAggregationService(
            ILogger<DataAggregationService> logger,
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

        public async Task<bool> AggregateAsync(CancellationToken cancellationToken = default)
        {
            var sensors = this._sensorService.GetSensors();
            if (sensors.Length == 0)
            {
                return false;
            }

            // Set the start date to the previous day, as today's data should not be processed yet (it's incomplete).
            var startDate = DateTime.Today.AddDays(-1);

            var processCount = 0;

            foreach (var sensor in sensors)
            {
                var failureCount = 0;

                for (int i = 0; i < 90; i++)
                {
                    var processDate = DateOnly.FromDateTime(startDate.AddDays(-i));
                    var succesful = await this.AggregateDateAsync(sensor, processDate);
                    if (!succesful)
                    {
                        this._logger.LogError($"{nameof(AggregateAsync)} - {sensor.DeviceId} {processDate}");
                        failureCount++;
                        continue;
                    }

                    processCount++;
                    this._logger.LogInformation($"{nameof(AggregateAsync)} - {sensor.DeviceId} {processDate}");
                }
            }

            if (processCount > 0)
            {
                return true;
            }

            return false;
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

            var key = $"{sensor.DeviceId}-{date:yyyy-MM-dd}.json";
            if (await this._objectStorageService.UploadFileAsync(key, memoryStream))
            {
                await this._objectStorageService.DeleteFilesAsync([.. sensorDataKeys]);
            }

            return true;
        }

        private async Task<UplinkMessageWebhook?> GetUplinkMessageWebhookAsync(string key)
        {
            var fileData = await this._objectStorageService.GetFileAsync(key);

            return JsonSerializer.Deserialize<UplinkMessageWebhook>(fileData, this._jsonSerializerOptions);
        }
    }
}
