using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using News_Data.Data;
using News_Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace News_Web_App
{
    public class VisitorCounterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<VisitorCounterMiddleware> _logger;

        public VisitorCounterMiddleware(RequestDelegate next, ILogger<VisitorCounterMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            try
            {
                // الحصول على عنوان IP الزائر
                var visitorIP = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = context.Request.Headers["User-Agent"].ToString();

                _logger.LogInformation($"Visitor IP: {visitorIP}, User-Agent: {userAgent}");

                // التحقق من وجود كوكي VisitorId
                if (!context.Request.Cookies.ContainsKey("VisitorId"))
                {
                    _logger.LogInformation("VisitorId cookie not found. Checking database...");

                    // التحقق من وجود الزائر في قاعدة البيانات بناءً على عنوان IP و User-Agent
                    var existingVisitor = await dbContext.Visitors
                        .FirstOrDefaultAsync(v => v.VisitorIP == visitorIP && v.UserAgent == userAgent);

                    if (existingVisitor == null)
                    {
                        _logger.LogInformation("Visitor not found in database. Adding new visitor...");

                        // إنشاء كائن Visitor جديد
                        var visitor = new Visitor
                        {
                            VisitorIP = visitorIP,
                            VisitDate = DateTime.UtcNow,
                            PageUrl = context.Request.Path,
                            UserAgent = userAgent,
                            IsUnique = true
                        };

                        // إضافة الزائر إلى قاعدة البيانات
                        await dbContext.Visitors.AddAsync(visitor);
                        await dbContext.SaveChangesAsync();

                        // إضافة كوكي VisitorId
                        context.Response.Cookies.Append("VisitorId", Guid.NewGuid().ToString(), new CookieOptions
                        {
                            Expires = DateTime.UtcNow.AddYears(1), // صلاحية الكوكي لمدة سنة
                            HttpOnly = true, // الكوكي غير قابل للوصول عبر JavaScript
                            Secure = true, // الكوكي يُرسل فقط عبر HTTPS
                            SameSite = SameSiteMode.Strict // يمنع إرسال الكوكي مع الطلبات الخارجية
                        });

                        _logger.LogInformation("New visitor added to database and cookie set.");
                    }
                    else
                    {
                        _logger.LogInformation("Visitor already exists in database.");

                        // تحديث تاريخ الزيارة إذا كان الزائر موجودًا بالفعل
                        existingVisitor.VisitDate = DateTime.UtcNow;
                        dbContext.Visitors.Update(existingVisitor);
                        await dbContext.SaveChangesAsync();

                        _logger.LogInformation("Visitor visit date updated.");
                    }
                }
                else
                {
                    _logger.LogInformation("VisitorId cookie found. Skipping...");
                }
            }
            catch (Exception ex)
            {
                // تسجيل الأخطاء في حالة حدوث مشكلة
                _logger.LogError($"Error in VisitorCounterMiddleware: {ex.Message}");
            }

            // الانتقال إلى الـ Middleware التالي
            await _next(context);
        }
    }
}