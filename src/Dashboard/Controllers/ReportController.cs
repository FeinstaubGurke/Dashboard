using Dashboard.Models;
using Dashboard.Services;
using FeinstaubGurke.PdfReport;
using FeinstaubGurke.PdfReport.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Dashboard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;
        private readonly SensorService _sensorService;
        private readonly IObjectStorageService _objectStorageService;

        public ReportController(
            ILogger<ReportController> logger,
            SensorService sensorService,
            IObjectStorageService objectStorageService)
        {
            this._logger = logger;
            this._sensorService = sensorService;
            this._objectStorageService = objectStorageService;
        }

        [HttpGet]
        [OutputCache(Duration = 30_000)] //6 hours
        public async Task<ActionResult<byte[]>> CreateReportAsync()
        {
            var sensors = this._sensorService.GetSensors();

            var startDate = DateTime.Today.AddDays(-1);
            var reportDays = 14;

            var items = new List<DeviceInfo>();

            foreach (var sensor in sensors)
            {
                var dailySensorRecords = new ConcurrentDictionary<DateOnly, SensorRecord[]>();

                var reportTasks = new List<Task>();

                for (var i = 0; i < reportDays; i++)
                {
                    var processingDate = DateOnly.FromDateTime(startDate.AddDays(-i));
                    var getReportTask = this.GetDayReportAsync(sensor, processingDate)
                        .ContinueWith(task =>
                        {
                            if (!task.IsCompletedSuccessfully)
                            {
                                return;
                            }

                            if (task.Result == null)
                            {
                                return;
                            }

                            dailySensorRecords.TryAdd(processingDate, task.Result);
                        });

                    reportTasks.Add(getReportTask);
                }

                await Task.WhenAll(reportTasks);

                items.Add(new DeviceInfo
                {
                    DeviceId = sensor.DeviceId,
                    Name = sensor.Name,
                    City = sensor.City,
                    District = sensor.District,
                    DailySensorRecords = dailySensorRecords.OrderBy(o => o.Key).ToDictionary()
                });
            }

            using var processor = new PdfProcessor(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/pdf"));
            var fileData = processor.CreateReport([.. items]);

            return File(fileData, "application/pdf", "report.pdf");
        }

        private async Task<SensorRecord[]?> GetDayReportAsync(
            Sensor sensor,
            DateOnly reportDate)
        {
            try
            {
                var objectKey = $"{sensor.DeviceId}-{reportDate:yyyy-MM-dd}.json";
                var dayJsonData = await this._objectStorageService.GetFileAsync(objectKey);
                var sensorDayData = JsonSerializer.Deserialize<SensorDayData>(dayJsonData);

                if (sensorDayData == null)
                {
                    return null;
                }

                return sensorDayData.SensorDetailRecords.Select(o => new SensorRecord
                {
                    Timestamp = o.Timestamp,
                    PM1 = o.PM1,
                    PM2_5 = o.PM2_5,
                    PM4 = o.PM4,
                    PM10 = o.PM10,
                    Humidity = o.Humidity,
                    Temperature = o.Temperature
                }).ToArray();
            }
            catch (Exception)
            {
                this._logger.LogInformation($"{nameof(GetDayReportAsync)} - Cannot get sensor data for {sensor.DeviceId} {reportDate}");
                return null;
            }
        }
    }
}
