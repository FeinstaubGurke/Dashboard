namespace Dashboard.Models
{
    public class Sensor
    {
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public string Status { get; set; }
        public DateTime? LastSignalReceivedTime { get; set; }
        public bool IsReady { get; set; }
        public double? PM1 { get; set; }
        public double? PM2_5 { get; set; }
    }
}
