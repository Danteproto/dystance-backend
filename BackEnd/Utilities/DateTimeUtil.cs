using System;
using System.Collections.Generic;

namespace BackEnd.Ultilities
{
    public class DateTimeUtil
    {
        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        public static DateTime GetDateTimeFromString(String str)
        {
            var date = str.ToDateTime("yyyy-M-ddTHH:mm:ss"); // {31.05.2016 13:33:00}

            return date;
        }
    }
}
