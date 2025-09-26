namespace Dashboard.Helpers
{
    public static class DateHelper
    {
        public static IEnumerable<DateOnly> GetDateRange(DateOnly start, DateOnly end)
        {
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                yield return date;
            }
        }
    }
}
