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

var dataProcessor = new DataProcessor();

foreach (var sensor in sensors)
{
    var records1 = await dataProcessor.SummarizeDataAsync(sensor, sensorDataPath);
    Console.WriteLine(records1.Count);

    var groupedDayRecords = records1.GroupBy(o => o.Timestamp.Date).Select(o => new { Key = o.Key, Items = o.ToList() });

    foreach (var dayRecords in groupedDayRecords)
    {
        var dayData = JsonSerializer.Serialize(dayRecords.Items);
        await File.WriteAllTextAsync(Path.Combine(sensorDataPath, $"{sensor.DeviceId}-{dayRecords.Key:yyyy-MM-dd}.json"), dayData);
    }
}


var files = Directory.GetFiles(sensorDataPath);
var records = new List<SensorRecord>();
Console.Write("Load webhook data from filesystem");

foreach (var file in files)
{
    var fileName = Path.GetFileNameWithoutExtension(file);
    if (fileName.Contains('_'))
    {
        continue;
    }

    var jsonData = await File.ReadAllBytesAsync(file);
    var sensorData = JsonSerializer.Deserialize<SensorRecord[]>(jsonData);
    records.AddRange(sensorData);
}

Console.WriteLine("Export csv report");
using (var writer = new StreamWriter("report.csv"))
using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
{
    csv.WriteRecords(records);
}

Console.WriteLine("Group data for pdf report");
var groupedDataByDeviceId = records.GroupBy(o => o.DeviceId).Select(o =>
{
    var sensorRecords = o.ToList();
    var sensor = sensorRecords.First();

    return new DeviceInfo
    {
        DeviceId = o.Key,
        Name = sensor.Name,
        City = sensor.City,
        District = sensor.District,
        Data = sensorRecords,
        HourlyPM2_5StatisticData = [.. CreateStatistic(sensorRecords, sensorRecord => sensorRecord.PM2_5)],
        HourGroupPM2_5StatisticData = [.. CreateHourGroupStatistic(sensorRecords, sensorRecord => sensorRecord.PM2_5)]
    };
}).ToArray();

Console.WriteLine("Create pdf report");
using var pdfHelper = new PdfHelper();
pdfHelper.CreateReport(groupedDataByDeviceId);






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

static IEnumerable<DayStatisticData> CreateHourGroupStatistic(List<SensorRecord> records, Func<SensorRecord, double?> field)
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
