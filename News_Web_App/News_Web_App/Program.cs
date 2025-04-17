using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using News_Data.Data;
using News_Data.Dbintializer;
using News_Data.EmailServices;
using News_Data.Implementation;
using News_Models.Model;
using News_Models.Repositories;
using News_Web_App;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// -------------------[ إعدادات رفع الملفات و Kestrel ]-------------------

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

// -------------------[ الخدمات الأساسية ]-------------------

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/AccessDenied";
    options.SlidingExpiration = true;
});

// -------------------[ الجلسة (Session) ]-------------------

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// -------------------[ الخدمات المخصصة ]-------------------

builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<IEmailSender, News_Data.EmailServices.EmailService>(); builder.Services.AddScoped<IUnitOfWork, UnitofWork>();
builder.Services.AddScoped<IDbintializer, Dbintializer>();

builder.Services.AddLogging();

var app = builder.Build();

// -------------------[ Middleware ]-------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// حظر صفحة التسجيل
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/Identity/Account/Register"))
    {
        context.Response.Redirect("/");
        return;
    }
    await next();
});

// عداد الزوار
app.UseMiddleware<VisitorCounterMiddleware>();

// -------------------[ المسارات ]-------------------

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// -------------------[ تهيئة قاعدة البيانات ]-------------------

SeedDatabase();

app.Run();

void SeedDatabase()
{
    using var scope = app.Services.CreateScope();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbintializer>();
    dbInitializer.Initialize().GetAwaiter().GetResult();
}
