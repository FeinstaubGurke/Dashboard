namespace Dashboard.Models
{
    public class Sensor
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime? LastSignalReceivedTime { get; set; }
    }
}
