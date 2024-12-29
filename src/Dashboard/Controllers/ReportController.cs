using Dashboard.Models;
using Dashboard.Services;
using FeinstaubGurke.PdfReport;
using FeinstaubGurke.PdfReport.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Dashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;
        private readonly SensorService _sensorService;
        private readonly IObjectStorageService _objectStorageService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

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
        [Route("")]
        public async Task<ActionResult<byte[]>> CreateReportAsync()
        {
            var sensors = this._sensorService.GetSensors();

            var startDate = DateTime.Today.AddDays(-1);
            var reportDays = 14;

            var items = new List<DeviceInfo>();

            foreach (var sensor in sensors)
            {
                var dailySensorRecords = new Dictionary<DateOnly, SensorRecord[]>();

                for (var i = 0; i < reportDays; i++)
                {
                    var processingDate = DateOnly.FromDateTime(startDate.AddDays(-i));

                    try
                    {
                        var dayJsonData = await this._objectStorageService.GetFileAsync($"{sensor.DeviceId}-{processingDate:yyyy-MM-dd}.json");
                        var sensorDayData = JsonSerializer.Deserialize<SensorDayData>(dayJsonData);

                        if (sensorDayData == null)
                        {
                            continue;
                        }

                        var sensorRecords = sensorDayData.SensorDetailRecords.Select(o => new SensorRecord
                        {
                            Timestamp = o.Timestamp,
                            PM1 = o.PM1,
                            PM2_5 = o.PM2_5,
                            PM4 = o.PM4,
                            PM10 = o.PM10,
                            Humidity = o.Humidity,
                            Temperature = o.Temperature
                        }).ToArray();

                        dailySensorRecords.Add(processingDate, sensorRecords);
                    }
                    catch (Exception exception)
                    {
                        this._logger.LogInformation($"{nameof(CreateReportAsync)} - Cannot get sensor data for {sensor.DeviceId}");
                        break;
                    }
                }

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
    }
}
