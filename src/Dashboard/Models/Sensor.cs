namespace Dashboard.Models
{
    public class Sensor
    {
        public string DeviceId { get; set; }
        public string Status { get; set; }
        public DateTime? LastSignalReceivedTime { get; set; }
        public bool IsReady { get; set; }
    }
}
