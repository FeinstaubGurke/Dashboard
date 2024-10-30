namespace CreateReport.Models
{
    public class DeviceInfo
    {
        public string DeviceId { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public List<SensorRecord> Data { get; set; }
        public HourlyStatisticData[] HourlyPM2_5StatisticData { get; set; }
    }
}