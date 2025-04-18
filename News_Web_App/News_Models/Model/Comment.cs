
namespace News_Models.Model
{
    public class Comment
    {
        public int Id { get; set; }
        public int NewsId { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public MessageStatus Status { get; set; } = MessageStatus.UnRead;
        public string? Reply { get; set; } // رد المسؤول على الرسالة 
        public News News { get; set; }

    }

}

