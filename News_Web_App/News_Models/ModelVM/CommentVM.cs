using News_Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News_Models.ModelVM
{
    public class CommentVM
    {
        public int Id { get; set; }
        public int NewsId { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public string CreatedAt { get; set; } 
        public MessageStatus Status { get; set; } = MessageStatus.UnRead;
        public string? Reply { get; set; } // رد المسؤول على الرسالة 
    }
}
