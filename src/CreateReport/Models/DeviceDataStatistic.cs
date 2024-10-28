namespace CreateReport.Models
{
    public class DeviceDataStatistic
    {
        public string DeviceId { get; set; }
        public List<CsvData> Data { get; set; }
        public HourlyStatisticData[] HourlyStatisticData { get; set; }
    }
}