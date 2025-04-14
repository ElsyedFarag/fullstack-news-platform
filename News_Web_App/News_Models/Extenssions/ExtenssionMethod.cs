using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News_Models.Extenssions
{
    public static class ExtenssionMethod
    {
        public static string TimeAgo( this DateTime dateTime)
        {
            TimeSpan timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalSeconds < 60)
                return "منذ لحظات";
            if (timeSpan.TotalMinutes < 60)
                return $"منذ {timeSpan.Minutes} دقيقة";
            if (timeSpan.TotalHours < 24)
                return $"منذ {timeSpan.Hours} ساعة";
            if (timeSpan.TotalDays < 7)
                return $"منذ {timeSpan.Days} يوم";
            if (timeSpan.TotalDays < 30)
                return $"منذ {timeSpan.Days / 7} أسبوع";
            if (timeSpan.TotalDays < 365)
                return $"منذ {timeSpan.Days / 30} شهر";

            return $"منذ {timeSpan.Days / 365} سنة";
        }
        public static string FormatDateOnly(DateOnly? date)
        {
            return date?.ToString("MMM. yyyy") ?? "Unknown";
        }
    }
}
