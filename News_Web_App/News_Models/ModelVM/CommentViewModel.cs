using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News_Models.ModelVM
{
    public class CommentViewModel
    {
        public int NewsId { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
    }
}
