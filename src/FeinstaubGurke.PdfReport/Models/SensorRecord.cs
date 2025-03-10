﻿namespace FeinstaubGurke.PdfReport.Models
{
    public class SensorRecord
    {
        public DateTime Timestamp { get; set; }
        public double? PM1 { get; set; }
        public double? PM2_5 { get; set; }
        public double? PM4 { get; set; }
        public double? PM10 { get; set; }
        public double? Temperature { get; set; }
        public double? Humidity { get; set; }
    }
}
