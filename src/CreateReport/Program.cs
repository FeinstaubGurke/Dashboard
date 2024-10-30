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

var records = new List<SensorRecord>();

Console.Write("Load webhook data from filesystem");
var i = 0;
var files = Directory.GetFiles(sensorDataPath);
foreach (var file in files)
{
    var jsonData = File.ReadAllBytes(file);
    var uplinkMessageWebhook = JsonSerializer.Deserialize<UplinkMessageWebhook>(jsonData, jsonSerializerOptions);

    records.Add(new SensorRecord
    {
        DeviceId = uplinkMessageWebhook.EndDeviceIds.DeviceId,
        Timestamp = uplinkMessageWebhook.ReceivedAt,
        PM1 = uplinkMessageWebhook.UplinkMessage.DecodedPayload.Decoded.PM1,
        PM2_5 = uplinkMessageWebhook.UplinkMessage.DecodedPayload.Decoded.PM2_5
    });

    i++;

    if (i % 1000 == 0)
    {
        Console.Write(".");
    }
}
Console.WriteLine("");

Console.WriteLine("Group data");
var groupedDataByDeviceId = records.GroupBy(o => o.DeviceId).Select(o =>
{
    var sensor = sensors.Where(sensor => sensor.DeviceId == o.Key);
    var sensorRecords = o.ToList();

    return new DeviceInfo
    {
        DeviceId = o.Key,
        Name = sensor.Select(sensor => sensor.Name).FirstOrDefault(),
        City = sensor.Select(sensor => sensor.City).FirstOrDefault(),
        District = sensor.Select(sensor => sensor.District).FirstOrDefault(),
        Data = sensorRecords,
        HourlyPM2_5StatisticData = [.. CreateStatistic(sensorRecords, sensorRecord => sensorRecord.PM2_5)]
    };
}).ToList();

Console.WriteLine("Create pdf report");
using var pdfHelper = new PdfHelper();
pdfHelper.CreateReport(groupedDataByDeviceId.ToArray());

//var groupedDataByDeviceId1 = records.GroupBy(o => new { o.DeviceId, o.Timestamp.Date }).Select(o => new
//{
//    DeviceId = o.Key.DeviceId,
//    Date = o.Key.Date,
//    PM1 = o.Select(o => o.PM1).Average(),
//    PM2_5 = o.Select(o => o.PM2_5).Average()
//});

//Console.WriteLine("Average Values by Date and Device");
//foreach (var data in groupedDataByDeviceId)
//{
//    var pm2_5GroupedByDate = data.Data.GroupBy(o => o.Timestamp.Date).Select(o => new { Date = o.Key, AvgPm2_5 = o.Average(o => o.PM2_5) });
//    foreach (var m in pm2_5GroupedByDate)
//    {
//        Console.WriteLine($"{data.DeviceId} {m.Date:yyyy-MM-dd} {m.AvgPm2_5:0.00}");
//    }
//}

//using (var writer = new StreamWriter("report.csv"))
//using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
//{
//    csv.WriteRecords(records);
//}


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
