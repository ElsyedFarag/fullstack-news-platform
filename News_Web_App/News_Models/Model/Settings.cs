using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace News_Models.Model
{
    public class Settings
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "يجب رفع شعار الموقع.")]
        public byte[]? Logo { get; set; }

        [NotMapped] // لا يتم تخزينها في قاعدة البيانات
        public IFormFile? LogoFile { get; set; }
        [Required(ErrorMessage = "عنوان الموقع مطلوب.")]
        [MaxLength(100, ErrorMessage = "يجب ألا يزيد العنوان عن 100 حرف.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "يجب إدخال وصف عن الموقع.")]
        public string? AboutUs { get; set; }

        [Url(ErrorMessage = "يرجى إدخال رابط صحيح.")]
        public string? Facebook { get; set; }

        [Url(ErrorMessage = "يرجى إدخال رابط صحيح.")]
        public string? Instagram { get; set; }

        [Url(ErrorMessage = "يرجى إدخال رابط صحيح.")]
        public string? Twitter { get; set; }

        [Url(ErrorMessage = "يرجى إدخال رابط صحيح.")]
        public string? TikTok { get; set; }

        // إضافة خصائص جديدة
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        [EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صحيح.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني المرسل مطلوب.")]
        [EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صحيح.")]
        public string? FromEmail { get; set; }

        [Required(ErrorMessage = "اسم المرسل مطلوب.")]
        public string? FromName { get; set; }
    }
}