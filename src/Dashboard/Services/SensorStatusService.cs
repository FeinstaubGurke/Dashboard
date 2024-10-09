using Dashboard.Models;
using System.Collections.Concurrent;

namespace Dashboard.Services
{
    public class SensorStatusService
    {
        private readonly ConcurrentDictionary<string, Sensor> _sensors = new ConcurrentDictionary<string, Sensor>();

        public SensorStatusService() { }

        public void SetTryJoin(string sensorId)
        {
            this._sensors.AddOrUpdate(sensorId, new Sensor
            {
                DeviceId = sensorId,
                Status = "Try join",
                LastSignalReceivedTime = DateTime.UtcNow,
                IsReady = false
            },
            (key, existingValue) =>
            {
                existingValue.Status = "Try join";
                existingValue.LastSignalReceivedTime = DateTime.UtcNow;
                existingValue.IsReady = false;

                return existingValue;
            });
        }

        public void UpdateStatus(string deviceId, string status)
        {
            this._sensors.AddOrUpdate(deviceId, new Sensor
            {
                DeviceId = deviceId,
                Status = status,
                LastSignalReceivedTime = DateTime.UtcNow,
                IsReady = true
            },
            (key, existingValue) =>
            {
                existingValue.Status = status;
                existingValue.LastSignalReceivedTime = DateTime.UtcNow;
                existingValue.IsReady = true;

                return existingValue;
            });
        }

        public Sensor[] GetSensors()
        {
            return [.. this._sensors.Values];
        }
    }
}
