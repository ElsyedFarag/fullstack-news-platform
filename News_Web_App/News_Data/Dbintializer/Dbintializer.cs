using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using News_Data.Data;
using News_Models.Model;
namespace News_Data.Dbintializer
{
    public class Dbintializer : IDbintializer
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public Dbintializer(AppDbContext context, UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager=userManager;
            _roleManager=roleManager;
        }

        public async Task Initialize() // ✅ تغيير `async void` إلى `async Task`
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration Error: {ex.Message}");
                throw;
            }

            // ✅ التحقق من عدم وجود بيانات مسبقة في الجدول باستخدام `await`
            if (!await _context.Settings.AnyAsync())
            {
                await _context.Settings.AddAsync(new Settings
                {
                    FromEmail = "test@example.com",
                    Username = "admin@gmail.com",
                    FromName = "Admin Name",
                    Facebook = "https://facebook.com",
                    TikTok = "https://tiktok.com",
                    AboutUs = "هذه بيانات تجريبية عن الموقع.",
                    Instagram = "https://instagram.com",
                    Twitter = "https://twitter.com",
                    Password = "123456",
                    Title = "تجربة الموقع",
                    Logo = new byte[] {},
                });
            }
            if (!_roleManager.RoleExistsAsync("مدير").GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole("مدير")).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole("مساعد مدير")).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole("محرر")).GetAwaiter().GetResult();


                _userManager.CreateAsync(new User
                {
                    UserName = "Admin@News.com",
                    Name = "المسئول",
                    Email = "Admin@News.com",
                    BirthDate = DateOnly.FromDateTime(new DateTime(2003, 4, 5)),
                    Gender = "Male",
                    PhoneNumber = "01011111111",
                    Address = "center,city",
                    EmailConfirmed = true,
                    ShowInfo = true,
                }, "P@$$w0rd").GetAwaiter().GetResult();

                User user = _context.Users.FirstOrDefault(x => x.Email == "Admin@News.com")!;
                _userManager.AddToRoleAsync(user, "مدير").GetAwaiter().GetResult();
                _userManager.GenerateEmailConfirmationTokenAsync(user).GetAwaiter().GetResult();
            }

            await _context.SaveChangesAsync(); // ✅ حفظ البيانات بشكل صحيح
        }
    }
}
