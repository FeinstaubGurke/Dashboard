namespace FeinstaubGurke.PdfReport.Models
{
    public class DayStatisticData
    {
        public DateOnly Date {  get; set; }
        public int HourGroup { get; set; }
        public double? Average { get; set; }
    }
}
