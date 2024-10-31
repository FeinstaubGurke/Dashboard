using CreateReport;
using CreateReport.Models;
using CsvHelper;
using Dashboard.Models;
using Dashboard.Models.Webhooks;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;


Console.WriteLine("Report Creator");

var sensorDataPath = @"C:\temp\FeinstaubDaten";

var jsonSerializerOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
};

Console.WriteLine("Get Sensor Informations");
using var httpClient = new HttpClient();
var sensors = await httpClient.GetFromJsonAsync<Sensor[]>("https://feinstaubgurke.at/sensor");
if (sensors == null)
{
    Console.WriteLine("Cannot load sensor data");
    return;
}

var sensorMapping = sensors.ToDictionary(o => o.DeviceId, o => o);

Console.Write("Load webhook data from filesystem");
var i = 0;
var files = Directory.GetFiles(sensorDataPath);
var records = new List<SensorRecord>(files.Length);

foreach (var file in files)
{
    var jsonData = await File.ReadAllBytesAsync(file);
    var uplinkMessageWebhook = JsonSerializer.Deserialize<UplinkMessageWebhook>(jsonData, jsonSerializerOptions);

    if (uplinkMessageWebhook == null)
    {
        continue;
    }

    var deviceId = uplinkMessageWebhook.EndDeviceIds.DeviceId;
    if (string.IsNullOrWhiteSpace(deviceId))
    {
        continue;
    }

    sensorMapping.TryGetValue(deviceId, out var sensor);
    sensor ??= new Sensor();

    records.Add(new SensorRecord
    {
        DeviceId = deviceId,
        City = sensor.City,
        District = sensor.District,
        Timestamp = uplinkMessageWebhook.ReceivedAt,
        PM1 = uplinkMessageWebhook.UplinkMessage.DecodedPayload.Decoded.PM1,
        PM2_5 = uplinkMessageWebhook.UplinkMessage.DecodedPayload.Decoded.PM2_5,
        PM4 = uplinkMessageWebhook.UplinkMessage.DecodedPayload.Decoded.PM4,
        PM10 = uplinkMessageWebhook.UplinkMessage.DecodedPayload.Decoded.PM10,
        Humidity = uplinkMessageWebhook.UplinkMessage.DecodedPayload.Decoded.Humidity,
        Temperature = uplinkMessageWebhook.UplinkMessage.DecodedPayload.Decoded.Temperature
    });

    i++;

    if (i % 1000 == 0)
    {
        Console.Write(".");
    }
}

Console.WriteLine("");

using (var writer = new StreamWriter("report.csv"))
using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
{
    csv.WriteRecords(records);
}

Console.WriteLine("Group data");
var groupedDataByDeviceId = records.GroupBy(o => o.DeviceId).Select(o =>
{
    sensorMapping.TryGetValue(o.Key, out var sensor);
    sensor ??= new Sensor();

    var sensorRecords = o.ToList();

    return new DeviceInfo
    {
        DeviceId = o.Key,
        Name = sensor.Name,
        City = sensor.City,
        District = sensor.District,
        Data = sensorRecords,
        HourlyPM2_5StatisticData = [.. CreateStatistic(sensorRecords, sensorRecord => sensorRecord.PM2_5)],
        HourGroupPM2_5StatisticData = [.. CreateStatistic1(sensorRecords, sensorRecord => sensorRecord.PM2_5)]
    };
}).ToList();

Console.WriteLine("Create pdf report");
using var pdfHelper = new PdfHelper();
pdfHelper.CreateReport(groupedDataByDeviceId.ToArray());


static IEnumerable<HourlyStatisticData> CreateStatistic(List<SensorRecord> records, Func<SensorRecord, double?> field)
{
    return records.GroupBy(o => new { o.Timestamp.Date, o.Timestamp.Hour }).Select(o =>
    {
        var recordsInThisHour = (double)o.Count();

        var veryGood = o.Where(x => field(x) >= 0 && field(x) < 5).Count();
        var good = o.Where(x => field(x) >= 5 && field(x) < 10).Count();
        var satisfactory = o.Where(x => field(x) >= 10 && field(x) < 15).Count();
        var poor = o.Where(x => field(x) >= 15 && field(x) < 20).Count();
        var veryPoor = o.Where(x => field(x) >= 20).Count();

        return new HourlyStatisticData
        {
            Date = DateOnly.FromDateTime(o.Key.Date),
            Hour = o.Key.Hour,
            VeryGood = veryGood / recordsInThisHour,
            Good = good / recordsInThisHour,
            Satisfactory = satisfactory / recordsInThisHour,
            Poor = poor / recordsInThisHour,
            VeryPoor = veryPoor / recordsInThisHour,
        };
    });
}

static int GetHourGroup(int hour)
{
    return hour / 2;
}

static IEnumerable<DayStatisticData> CreateStatistic1(List<SensorRecord> records, Func<SensorRecord, double?> field)
{
    return records.GroupBy(o => new { o.Timestamp.Date, Group = GetHourGroup(o.Timestamp.Hour) }).Select(o =>
    {
        var hourGroupAverage = o.Select(x => field(x)).Average();

        return new DayStatisticData
        {
            Date = DateOnly.FromDateTime(o.Key.Date),
            HourGroup = o.Key.Group,
            Average = hourGroupAverage
        };
    });
}
