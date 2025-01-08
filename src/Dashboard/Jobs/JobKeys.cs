using Quartz;

namespace Dashboard.Jobs
{
    /// <summary>
    /// Job Keys
    /// </summary>
    public class JobKeys
    {
        /// <summary>
        /// Sensor Inactivity Check
        /// </summary>
        public static JobKey SensorInactivityCheck = new JobKey(nameof(SensorInactivityCheck));

        /// <summary>
        /// Aggregate Sensor Data
        /// </summary>
        public static JobKey AggregateSensorData = new JobKey(nameof(AggregateSensorData));
    }
}
