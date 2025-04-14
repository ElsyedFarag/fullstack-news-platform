using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using News_Models;
using News_Models.Model;
using News_Models.Models;

namespace News_Data.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : 
        IdentityDbContext<User>(options)
    {
        public required DbSet<News> News { get; set; }
        public required DbSet<Message> Messages { get; set; }
        public required DbSet<Settings> Settings { get; set; }
        public required DbSet<Comment> Comments { get; set; }
        public DbSet<Visitor> Visitors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تحديد الطول الأقصى للحقل لتجنب تجاوز الحجم في قاعدة البيانات
            modelBuilder.Entity<News>()
                .Property(n => n.Title)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<Message>()
                .Property(m => m.Email)
                .HasMaxLength(150)
                .IsRequired();

            // إنشاء فهرس لتحسين البحث
            modelBuilder.Entity<News>()
                .HasIndex(n => n.Created);

            modelBuilder.Entity<Message>()
                .HasIndex(m => m.Status);
        }
    }
}
