using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using News_Models.Model;
using News_Models.Repositories;

namespace News_Data.EmailServices
{
    public class EmailService : IEmailSender
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailService> _logger;
        private static readonly SmtpClient _smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            EnableSsl = true
        };

        public EmailService(IServiceProvider serviceProvider, ILogger<EmailService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var emailSettings = (await unitOfWork.GetRepository<Settings>().GetAllAsync()).FirstOrDefault();


                if (emailSettings == null)
                {
                    _logger.LogError("❌ لم يتم العثور على إعدادات البريد الإلكتروني في قاعدة البيانات.");
                    throw new InvalidOperationException("لم يتم العثور على إعدادات البريد الإلكتروني.");
                }

                _smtpClient.Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password);

                using var message = new MailMessage
                {
                    From = new MailAddress(emailSettings.Username),
                    Subject = subject,
                    Body = $"<html><body>{htmlMessage}</body></html>",
                    IsBodyHtml = true
                };

                message.To.Add(email);

                await _smtpClient.SendMailAsync(message);
                _logger.LogInformation("✅ تم إرسال البريد الإلكتروني بنجاح إلى {Email}", email);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError("⚠️ خطأ في SMTP: {Message}", smtpEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ خطأ غير متوقع أثناء إرسال البريد: {Message}", ex.Message);
            }
        }
    }
}
