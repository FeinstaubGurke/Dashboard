using CreateReport;
using CreateReport.Models;
using CsvHelper;
using Dashboard.Models.Webhooks;
using System.Globalization;
using System.Text.Json;


Console.WriteLine("Report Creator");

var sensorDataPath = @"C:\temp\FeinstaubDaten";

var jsonSerializerOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
};

var records = new List<CsvData>();

var files = Directory.GetFiles(sensorDataPath);
foreach (var file in files)
{
    var jsonData = File.ReadAllBytes(file);
    var uplinkMessageWebhook = JsonSerializer.Deserialize<UplinkMessageWebhook>(jsonData, jsonSerializerOptions);

    records.Add(new CsvData
    {
        DeviceId = uplinkMessageWebhook.EndDeviceIds.DeviceId,
        Timestamp = uplinkMessageWebhook.ReceivedAt,
        PM1 = uplinkMessageWebhook.UplinkMessage.DecodedPayload.Decoded.PM1,
        PM2_5 = uplinkMessageWebhook.UplinkMessage.DecodedPayload.Decoded.PM2_5
    });
}

var groupedDataByDeviceId = records.GroupBy(o => o.DeviceId).Select(o => new DeviceDataStatistic
{
    DeviceId = o.Key,
    Data = o.ToList(),
    HourlyStatisticData = [..CreateStatistic(o.ToList(), csvData => csvData.PM2_5)]
}).ToList();


PdfHelper.Draw(groupedDataByDeviceId.ToArray());

var groupedDataByDeviceId1 = records.GroupBy(o => new { o.DeviceId, o.Timestamp.Date }).Select(o => new
{
    DeviceId = o.Key.DeviceId,
    Date = o.Key.Date,
    PM1 = o.Select(o => o.PM1).Average(),
    PM2_5 = o.Select(o => o.PM2_5).Average()
});

Console.WriteLine("Average Values by Date and Device");
foreach (var data in groupedDataByDeviceId)
{
    var pm2_5GroupedByDate = data.Data.GroupBy(o => o.Timestamp.Date).Select(o => new { Date = o.Key, AvgPm2_5 = o.Average(o => o.PM2_5) });
    foreach (var m in pm2_5GroupedByDate)
    {
        Console.WriteLine($"{data.DeviceId} {m.Date:yyyy-MM-dd} {m.AvgPm2_5:0.00}");
    }
}


using (var writer = new StreamWriter("report.csv"))
using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
{
    csv.WriteRecords(records);
}


static IEnumerable<HourlyStatisticData> CreateStatistic(List<CsvData> records, Func<CsvData, double?> field)
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
