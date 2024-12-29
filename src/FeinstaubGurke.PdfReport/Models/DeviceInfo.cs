namespace FeinstaubGurke.PdfReport.Models
{
    public class DeviceInfo
    {
        public string DeviceId { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }

        public Dictionary<DateOnly, SensorRecord[]> DailySensorRecords { get; set; }
    }
}