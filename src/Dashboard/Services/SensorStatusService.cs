using System.Collections.Concurrent;

namespace Dashboard.Services
{
    public class SensorStatusService
    {
        private readonly ConcurrentDictionary<string, string> _sensors = new ConcurrentDictionary<string, string>();

        public SensorStatusService() { }

        public void UpdateStatus(string sensorId, string status)
        {
            this._sensors.AddOrUpdate(sensorId, status, (key, oldValue) => oldValue = status);
        }

        public string[] GetSensors()
        {
            return this._sensors.Select(sensor => $"{sensor.Key} {sensor.Value}").ToArray();
        }
    }
}
