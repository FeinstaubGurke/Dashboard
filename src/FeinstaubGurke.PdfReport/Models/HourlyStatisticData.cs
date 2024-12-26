namespace FeinstaubGurke.PdfReport.Models
{
    public class HourlyStatisticData
    {
        public DateOnly Date { get; set; }
        public int Hour { get; set; }
        public double VeryGood { get; set; }
        public double Good { get; set; }
        public double Satisfactory { get; set; }
        public double Poor { get; set; }
        public double VeryPoor { get; set; }
    }
}
