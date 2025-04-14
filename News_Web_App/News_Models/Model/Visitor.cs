using System;

namespace News_Models.Models
{
    public class Visitor
    {
        public int VisitorId { get; set; } // معرف الزائر
        public string VisitorIP { get; set; } // عنوان IP الخاص بالزائر
        public DateTime VisitDate { get; set; } // تاريخ ووقت الزيارة
        public string PageUrl { get; set; } // الصفحة التي تمت زيارتها
        public string UserAgent { get; set; } // معلومات المتصفح ونظام التشغيل
        public bool IsUnique { get; set; } // هل الزائر فريد (اختياري)
    }
}