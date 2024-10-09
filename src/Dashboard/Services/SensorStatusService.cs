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
            this._sensors.AddOrUpdate(sensorId, new Sensor { Name = sensorId, Status = "Try join" }, (key, existingValue) =>
            {
                existingValue.Status = "Try join";
                existingValue.LastSignalReceivedTime = null;

                return existingValue;
            });
        }

        public void UpdateStatus(string sensorId, string status)
        {
            this._sensors.AddOrUpdate(sensorId, new Sensor { Name = sensorId, Status = status }, (key, existingValue) =>
            {
                existingValue.Status = status;
                existingValue.LastSignalReceivedTime = DateTime.UtcNow;

                return existingValue;
            });
        }

        public Sensor[] GetSensors()
        {
            return [.. this._sensors.Values];
        }
    }
}
