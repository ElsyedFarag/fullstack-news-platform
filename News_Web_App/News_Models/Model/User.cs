using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace News_Models.Model
{
    public class User : IdentityUser
    {
        [Required]
        public string Name { get; set; }

        [MaxLength(7)]
        public string Gender { get; set; }

        public string? Address { get; set; }

        [MaxLength(100)]
        public string? About { get; set; }

        public string? Facebook { get; set; }

        public string? GitHub { get; set; }

        public string? Instagram { get; set; }

        public string? Twitter { get; set; }

        public string? Linkedin { get; set; }

        public DateOnly? BirthDate { get; set; }

        public DateOnly? Created { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public byte[]? ImageUrl { get; set; }

        public bool IsDeleted { get; set; }
        public bool ShowInfo { get; set; }
    }
}
