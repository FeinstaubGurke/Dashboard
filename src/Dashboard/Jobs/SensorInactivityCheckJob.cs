using Dashboard.Services;
using Quartz;

namespace Dashboard.Jobs
{
    /// <summary>
    /// Sensor Inactivity Check Job
    /// </summary>
    [DisallowConcurrentExecution]
    public class SensorInactivityCheckJob : IJob
    {
        private readonly SensorService _sensorService;

        /// <summary>
        /// Sensor Inactivity Check Job
        /// </summary>
        public SensorInactivityCheckJob(SensorService sensorService)
        {
            this._sensorService = sensorService;
        }

        /// <inheritdoc/>
        public async Task Execute(IJobExecutionContext context)
        {
            var sensors = this._sensorService.GetSensors();
            var inactivityDuration = TimeSpan.FromHours(1);

            foreach (var sensor in sensors)
            {
                if (!sensor.LastSignalReceivedTime.HasValue)
                {
                    continue;
                }

                if (sensor.LastSignalReceivedTime.Value.Add(inactivityDuration) > DateTime.UtcNow)
                {
                    continue;
                }

                this._sensorService.SetOffline(sensor.DeviceId);
            }
        }
    }
}
