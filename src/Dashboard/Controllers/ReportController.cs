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

            var items = new List<DeviceInfo>();

            foreach (var sensor in sensors)
            {
                var sensorRecords = new List<SensorRecord>();

                for (var i = 0; i < 14; i++)
                {
                    var processingDate = startDate.AddDays(-i);

                    try
                    {
                        var dayJsonData = await this._objectStorageService.GetFileAsync($"{sensor.DeviceId}-{processingDate:yyyy-MM-dd}.json");
                        var sensorDayData = JsonSerializer.Deserialize<SensorDayData>(dayJsonData);

                        sensorRecords.AddRange(sensorDayData.SensorDetailRecords.Select(o => new SensorRecord
                        {
                            DeviceId = sensor.DeviceId,
                            Name = sensor.Name,
                            City = sensor.City,
                            District = sensor.District,
                            Timestamp = o.Timestamp,
                            PM1 = o.PM1,
                            PM2_5 = o.PM2_5,
                            PM4 = o.PM4,
                            PM10 = o.PM10,
                            Humidity = o.Humidity,
                            Temperature = o.Temperature
                        }).ToArray());

                        this._logger.LogInformation(sensorDayData.SensorDetailRecords.Length.ToString());
                    }
                    catch (Exception exception)
                    {
                        break;
                    }
                }


                items.Add(new DeviceInfo
                {
                    DeviceId = sensor.DeviceId,
                    Name = sensor.Name,
                    City = sensor.City,
                    District = sensor.District,
                    Data = sensorRecords,
                    HourlyPM2_5StatisticData = [.. DataHelper.CreateStatistic(sensorRecords.ToList(), sensorRecord => sensorRecord.PM2_5)],
                    HourGroupPM2_5StatisticData = [.. DataHelper.CreateHourGroupStatistic(sensorRecords.ToList(), sensorRecord => sensorRecord.PM2_5)],
                });
            }



            using var processor = new PdfProcessor(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/pdf"));
            var fileData = processor.CreateReport(items.ToArray());

            return File(fileData, "application/pdf", "report.pdf");
        }
    }
}
