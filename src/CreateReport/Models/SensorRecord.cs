namespace CreateReport.Models
{
    public class SensorRecord
    {
        public string City { get; set; }
        public string District { get; set; }
        public string DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public double? PM1 { get; set; }
        public double? PM2_5 { get; set; }
        public double? PM4 { get; set; }
        public double? PM10 { get; set; }
        public double? Temperature { get; set; }
        public double? Humidity { get; set; }
    }
}
