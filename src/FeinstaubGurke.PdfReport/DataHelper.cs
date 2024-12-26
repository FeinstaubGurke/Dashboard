using FeinstaubGurke.PdfReport.Models;

namespace FeinstaubGurke.PdfReport
{
    public static class DataHelper
    {
        public static IEnumerable<HourlyStatisticData> CreateStatistic(
            List<SensorRecord> records,
            Func<SensorRecord, double?> field)
        {
            return records.GroupBy(o => new { o.Timestamp.Date, o.Timestamp.Hour }).Select(o =>
            {
                var recordsInThisHour = (double)o.Count();

                var veryGood = o.Where(x => field(x) >= 0 && field(x) < 5).Count();
                var good = o.Where(x => field(x) >= 5 && field(x) < 10).Count();
                var satisfactory = o.Where(x => field(x) >= 10 && field(x) < 15).Count();
                var poor = o.Where(x => field(x) >= 15 && field(x) < 20).Count();
                var veryPoor = o.Where(x => field(x) >= 20).Count();

                return new HourlyStatisticData
                {
                    Date = DateOnly.FromDateTime(o.Key.Date),
                    Hour = o.Key.Hour,
                    VeryGood = veryGood / recordsInThisHour,
                    Good = good / recordsInThisHour,
                    Satisfactory = satisfactory / recordsInThisHour,
                    Poor = poor / recordsInThisHour,
                    VeryPoor = veryPoor / recordsInThisHour,
                };
            });
        }

        public static int GetHourGroup(int hour)
        {
            return hour / 2;
        }

        public static IEnumerable<DayStatisticData> CreateHourGroupStatistic(
            List<SensorRecord> records,
            Func<SensorRecord, double?> field)
        {
            return records.GroupBy(o => new { o.Timestamp.Date, Group = GetHourGroup(o.Timestamp.Hour) }).Select(o =>
            {
                var hourGroupAverage = o.Select(x => field(x)).Average();

                return new DayStatisticData
                {
                    Date = DateOnly.FromDateTime(o.Key.Date),
                    HourGroup = o.Key.Group,
                    Average = hourGroupAverage
                };
            });
        }

    }
}
