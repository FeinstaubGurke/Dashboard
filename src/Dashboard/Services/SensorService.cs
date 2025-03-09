using Dashboard.Clients;
using Dashboard.Models;
using System.Collections.Concurrent;

namespace Dashboard.Services
{
    public class SensorService
    {
        private readonly ConcurrentDictionary<string, Sensor> _sensors = new ConcurrentDictionary<string, Sensor>();

        public SensorService(TheThingsStackClient theThingsStackClient)
        {
            var endDevices = theThingsStackClient.GetDevicesAsync().GetAwaiter().GetResult();
            if (endDevices == null)
            {
                throw new Exception("TheThingsStackClient failure");
            }

            foreach (var device in endDevices)
            {
                this._sensors.TryAdd(device.ids.device_id, new Sensor
                {
                    DeviceId = device.ids.device_id,
                    Name = device.name,
                    Description = device.description,
                    City = device.attributes?.city ?? "",
                    District = device.attributes?.district ?? "",
                    Status = "unknown"
                });
            }
        }

        public void SetTryJoin(string deviceId)
        {
            var status = "Try join";

            this.UpdateStatus(deviceId, status);
        }

        public void SetOffline(string deviceId)
        {
            if (this._sensors.TryGetValue(deviceId, out var existingValue))
            {
                existingValue.Status = "unknown";
                existingValue.IsReady = false;
            }
        }

        private void UpdateStatus(string deviceId, string status)
        {
            this._sensors.AddOrUpdate(deviceId, new Sensor
            {
                DeviceId = deviceId,
                Status = status,
                LastSignalReceivedTime = DateTime.UtcNow,
                IsReady = false,
                PM1 = null,
                PM2_5 = null

            },
            (key, existingValue) =>
            {
                existingValue.Status = status;
                existingValue.LastSignalReceivedTime = DateTime.UtcNow;
                existingValue.IsReady = false;
                existingValue.PM1 = null;
                existingValue.PM2_5 = null;

                return existingValue;
            });
        }

        public void UpdateSensorData(
            string deviceId,
            string txReason,
            double? pm1,
            double? pm2_5)
        {
            var status = "unknown";
            if (txReason.Equals("Timer", StringComparison.OrdinalIgnoreCase))
            {
                status = "Active";
            }
            else if (txReason.Equals("Joined", StringComparison.OrdinalIgnoreCase))
            {
                status = "Ready";
            }

            this._sensors.AddOrUpdate(deviceId, new Sensor
            {
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
            return [.. this._sensors.Values.OrderBy(sensor => sensor.City).ThenBy(sensor => sensor.District)];
        }
    }
}
