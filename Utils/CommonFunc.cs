using WebApp.Payloads;

namespace WebApp.Utils;

public class CommonFunc
{
    public static List<int> GetMonthsInRange(DateTime fromDate, DateTime toDate)
    {
        List<int> months = [];

        // Ensure fromDate is before toDate
        if (fromDate > toDate)
        {
            (fromDate, toDate) = (toDate, fromDate);
        }

        int currentMonth = fromDate.Month;
        int currentYear = fromDate.Year;

        while (new DateTime(currentYear, currentMonth, 1) <= toDate)
        {
            months.Add(currentMonth);
            currentMonth++;
            if (currentMonth > 12)
            {
                currentMonth = 1;
                currentYear++;
            }
        }

        return months;
    }

    public static List<DateRange> SplitDateRange(DateTime startDate, DateTime endDate)
    {
        List<DateRange> dateRanges = [];

        DateTime currentStart = startDate;

        while (currentStart <= endDate)
        {
            DateTime currentEnd = new DateTime(currentStart.Year, currentStart.Month,
                                               DateTime.DaysInMonth(currentStart.Year, currentStart.Month));

            if (currentEnd > endDate)
            {
                currentEnd = endDate;
            }

            dateRanges.Add(new DateRange
            {
                FromDate = currentStart,
                ToDate = currentEnd
            });

            currentStart = currentEnd.AddDays(1);
        }

        return dateRanges;
    }
}