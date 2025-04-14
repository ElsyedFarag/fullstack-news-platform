using System;
using System.ComponentModel.DataAnnotations;

namespace News_Models.Model
{
    public class Message
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [StringLength(50, ErrorMessage = "يجب ألا يزيد الاسم عن 50 حرفًا")]
        public string Name { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "يجب إدخال بريد إلكتروني صالح")]
        public string Email { get; set; }

        [Required(ErrorMessage = "الرسالة مطلوبة")]
        [StringLength(500, ErrorMessage = "يجب ألا تزيد الرسالة عن 500 حرف")]
        public string Content { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get;  set; } = DateTime.UtcNow;

        public MessageStatus Status { get; set; } = MessageStatus.UnRead;

        public string? Reply { get; set; } // رد المسؤول على الرسالة
        public bool IsArchived { get; set; } = false; // هل تمت أرشفة الرسالة؟

    }

    public enum MessageStatus
    {
        None,
        Read,
        UnRead
    }
}
