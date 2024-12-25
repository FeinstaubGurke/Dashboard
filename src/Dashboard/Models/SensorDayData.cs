namespace Dashboard.Models
{
    public class SensorDayData
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string DeviceId { get; set; }
        public DateOnly Date { get; set; }

        public SensorAverage Average { get; set; }

        public SensorDetailRecord[] SensorDetailRecords { get; set; }
    }
}
