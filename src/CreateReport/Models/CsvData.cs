namespace CreateReport.Models
{
    public class CsvData
    {
        public string DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public double? PM2_5 { get; set; }
        public double? PM1 { get; set; }
    }
}
