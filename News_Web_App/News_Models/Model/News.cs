using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace News_Models.Model
{
    public class News
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "العنوان مطلوب.")]
        [MaxLength(255, ErrorMessage = "العنوان يجب ألا يتجاوز 255 حرفًا.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "الوصف مطلوب.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المؤلف مطلوب.")]
        [MaxLength(100, ErrorMessage = "اسم المؤلف يجب ألا يتجاوز 100 حرفًا.")]
        public string Author { get; set; } = string.Empty;

        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;

        public int ViewCount { get; set; }
        public int Likes { get; set; }
        // ✅ العلاقة: خبر يحتوي على تعليقات
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        [ValidateNever]
        public string? VideoFile { get; set; } // سيتم تخزين مسار الفيديو هنا

        [NotMapped]
        public IFormFile? VideoUpload { get; set; } // لاستقبال الملف من النموذج

        [Required(ErrorMessage = "حالة الخبر مطلوبة.")]
        public NewsStatus Status { get; set; } = NewsStatus.Draft;
    }

    public enum NewsStatus
    {
        Draft,
        Published,
        Archived
    }
}