using CreateReport.Models;
using Dashboard.Models;
using Dashboard.Models.Webhooks;
using System.Globalization;
using System.Text.Json;

namespace CreateReport
{
    internal class DataProcessor
    {
        private JsonSerializerOptions _jsonSerializerOptions;

        public DataProcessor()
        {
            this._jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
        }

        public async Task<List<SensorRecord>> SummarizeDataAsync(
            Sensor sensor,
            string sensorDataPath)
        {
            var files = Directory.GetFiles(sensorDataPath, $"{sensor.DeviceId}*");
            var records = new List<SensorRecord>(files.Length);
            var i = 0;

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var dateDataPart = fileName.Substring(sensor.DeviceId.Length + 1);
                if (!DateTime.TryParseExact(dateDataPart, "yyyy-MM-dd_HH_mm", null, DateTimeStyles.None, out var date))
                {
                    continue;
                }

                var jsonData = await File.ReadAllBytesAsync(file);
                var uplinkMessageWebhook = JsonSerializer.Deserialize<UplinkMessageWebhook>(jsonData, this._jsonSerializerOptions);

                if (uplinkMessageWebhook == null)
                {
                    continue;
                }

                var deviceId = uplinkMessageWebhook.EndDeviceIds.DeviceId;
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    continue;
                }

                records.Add(new SensorRecord
                {
                    Name = sensor.Name,
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

            return records;
        }
    }
}
