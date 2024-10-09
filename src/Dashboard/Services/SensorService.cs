using Dashboard.Models;
using System.Collections.Concurrent;

namespace Dashboard.Services
{
    public class SensorService
    {
        private readonly ConcurrentDictionary<string, Sensor> _sensors = new ConcurrentDictionary<string, Sensor>();
        private readonly Dictionary<string, string> _sensorMapping = new Dictionary<string, string>();

        public SensorService()
        {
            this._sensorMapping.Add("eui-7066e1fffe0112ce", "Sensor FG01");
            this._sensorMapping.Add("eui-7066e1fffe0112ae", "Sensor FG02");
            this._sensorMapping.Add("eui-7066e1fffe01121e", "Sensor FG03");
            this._sensorMapping.Add("eui-7066e1fffe0112d2", "Sensor FG04");
            this._sensorMapping.Add("eui-7066e1fffe011224", "Sensor FG05");
            this._sensorMapping.Add("eui-7066e1fffe0112ac", "Sensor FG06");
        }

        private bool TryGetSensorName(string deviceId, out string? sensorName)
        {
            return this._sensorMapping.TryGetValue(deviceId, out sensorName);
        }

        public void SetTryJoin(string deviceId)
        {
            if (!this.TryGetSensorName(deviceId, out var sensorName))
            {
                return;
            }

            this._sensors.AddOrUpdate(deviceId, new Sensor
            {
                Name = sensorName,
                DeviceId = deviceId,
                Status = "Try join",
                LastSignalReceivedTime = DateTime.UtcNow,
                IsReady = false,
                PM1 = null,
                PM2_5 = null

            },
            (key, existingValue) =>
            {
                existingValue.Status = "Try join";
                existingValue.LastSignalReceivedTime = DateTime.UtcNow;
                existingValue.IsReady = false;
                existingValue.PM1 = null;
                existingValue.PM2_5 = null;

                return existingValue;
            });
        }

        public void UpdateStatus(
            string deviceId,
            string txReason,
            double? pm1,
            double? pm2_5)
        {
            if (!this.TryGetSensorName(deviceId, out var sensorName))
            {
                return;
            }

            var status = "unknown";
            if (txReason.Equals("Timer", StringComparison.OrdinalIgnoreCase))
            {
                status = "Active";
            }

            this._sensors.AddOrUpdate(deviceId, new Sensor
            {
                Name = sensorName,
                DeviceId = deviceId,
                Status = status,
                LastSignalReceivedTime = DateTime.UtcNow,
                IsReady = true,
                PM1 = pm1,
                PM2_5 = pm2_5,
            },
            (key, existingValue) =>
            {
                existingValue.Status = status;
                existingValue.LastSignalReceivedTime = DateTime.UtcNow;
                existingValue.IsReady = true;
                existingValue.PM1 = pm1;
                existingValue.PM2_5 = pm2_5;

                return existingValue;
            });
        }

        public Sensor[] GetSensors()
        {
            return [.. this._sensors.Values.OrderBy(sensor => sensor.Name)];
        }
    }
}
