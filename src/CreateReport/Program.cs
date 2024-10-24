// See https://aka.ms/new-console-template for more information
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

var groupedDataByDeviceId = records.GroupBy(o => o.DeviceId).Select(o => new
{
    DeviceId = o.Key,
    Data = o.ToList()
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


public class CsvData
{
    public string DeviceId { get; set; }
    public DateTime Timestamp { get; set; }
    public double? PM2_5 { get; set; }
    public double? PM1 { get; set; }
}