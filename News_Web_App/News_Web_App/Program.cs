using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using News_Data.Data;
using News_Data.Dbintializer;
using News_Data.Implementation;
using News_Models.Repositories;
using News_Models.Model;
using News_Web_App;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Http.Features;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// تكوين إعدادات تحميل الملفات
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 ميجابايت
});

// تكوين Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600; // 100 ميجابايت
});

builder.Services.AddControllersWithViews();

// تسجيل Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // تأكيد الحساب مطلوب
    options.User.RequireUniqueEmail = true; // البريد الإلكتروني فريد
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI(); // تعطيل واجهة المستخدم الافتراضية للتسجيل

// تسجيل AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    IConfigurationSection googleAuthSection = builder.Configuration.GetSection("Authentication:Google");

                    options.ClientId = googleAuthSection["ClientId"]!;
                    options.ClientSecret = googleAuthSection["ClientSecret"]!;
                })
                .AddMicrosoftAccount(microsoftOptions =>
                {
                    microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
                    microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
                });
// تسجيل خدمات أخرى
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // أو ما يتناسب مع احتياجاتك
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // لجعل الـ Session متاحاً حتى في سياسة الخصوصية
}); 

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // صلاحية الكوكي لمدة 30 يومًا
    options.LoginPath = "/Login"; // مسار صفحة تسجيل الدخول
    options.AccessDeniedPath = "/AccessDenied"; // مسار صفحة الوصول المرفوض
    options.SlidingExpiration = true; // تجديد صلاحية الكوكي تلقائيًا عند الاستخدام
});

builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<IEmailSender, News_Data.EmailServices.EmailService>();
builder.Services.AddScoped<IUnitOfWork, UnitofWork>();
builder.Services.AddScoped<IDbintializer, Dbintializer>();

// تسجيل Logging
builder.Services.AddLogging();

var app = builder.Build();

// إعدادات الـ Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // تفعيل HTTPS في البيئة الإنتاجية
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // تفعيل الملفات الثابتة (CSS, JS, Images)

app.UseSession(); // تفعيل الجلسة
app.UseRouting();
app.UseAuthentication(); // تفعيل المصادقة (Authentication)
app.UseAuthorization(); // تفعيل التصريح (Authorization)

// Middleware لمنع الوصول إلى صفحة التسجيل
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/Identity/Account/Register"))
    {
        context.Response.Redirect("/"); // إعادة التوجيه إلى الصفحة الرئيسية
        return;
    }
    await next();
});

// تفعيل VisitorCounterMiddleware
app.UseMiddleware<VisitorCounterMiddleware>();

// تعيين مسارات التحكم
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// تهيئة قاعدة البيانات
SpeedUp();

app.Run();

void SpeedUp()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbintializer>();
        dbInitializer.Initialize().GetAwaiter().GetResult();
    }
}