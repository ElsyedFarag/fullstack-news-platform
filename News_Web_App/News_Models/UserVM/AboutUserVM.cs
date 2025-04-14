using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News_Models.UserVM
{
    public  class AboutUserVM
    {

        [Required]
        public string Name { get; set; }
        public string? RoleUSer { get; set; }
        [MaxLength(100)]
        public string? About { get; set; }

        public string? Facebook { get; set; }

        public string? GitHub { get; set; }

        public string? Instagram { get; set; }

        public string? Twitter { get; set; }

        public string? Linkedin { get; set; }
        public byte[]? ImageUrl { get; set; }

    }
}
