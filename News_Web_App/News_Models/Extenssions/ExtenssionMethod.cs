using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News_Models.Extenssions
{
    public static class ExtenssionMethod
    {

        public static string TimeAgo(this DateTime dateTime)
        {
            TimeSpan timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalSeconds < 60)
                return "منذ لحظات";

            if (timeSpan.TotalMinutes < 60)
                return $"منذ {GetArabicPlural((int)timeSpan.TotalMinutes, "دقيقة", "دقيقتين", "دقائق", "دقيقة")}";

            if (timeSpan.TotalHours < 24)
                return $"منذ {GetArabicPlural((int)timeSpan.TotalHours, "ساعة", "ساعتين", "ساعات", "ساعة")}";

            if (timeSpan.TotalDays < 7)
                return $"منذ {GetArabicPlural((int)timeSpan.TotalDays, "يوم", "يومين", "أيام", "يوم")}";

            if (timeSpan.TotalDays < 30)
                return $"منذ {GetArabicPlural((int)(timeSpan.TotalDays / 7), "أسبوع", "أسبوعين", "أسابيع", "أسبوع")}";

            if (timeSpan.TotalDays < 365)
                return $"منذ {GetArabicPlural((int)(timeSpan.TotalDays / 30), "شهر", "شهرين", "أشهر", "شهر")}";

            return $"منذ {GetArabicPlural((int)(timeSpan.TotalDays / 365), "سنة", "سنتين", "سنوات", "سنة")}";
        }

        private static string GetArabicPlural(int value, string singular, string dual, string plural, string singularWithNumber)
        {
            if (value == 1)
                return $" {singular}";
            if (value == 2)
                return $" {dual}";
            if (value >= 3 && value <= 10)
                return $"{value} {plural}";
            return $"{value} {singularWithNumber}";
        }
        

        public static string FormatDateOnly(DateOnly? date)
        {
            return date?.ToString("MMM. yyyy") ?? "Unknown";
        }
    }
}
