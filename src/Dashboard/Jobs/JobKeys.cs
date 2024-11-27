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
    }
}
