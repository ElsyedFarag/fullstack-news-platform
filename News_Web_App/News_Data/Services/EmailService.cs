using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using News_Models.Model;
using News_Models.Repositories;
using System.Net.Mail;
using System.Net;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public EmailService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var emailSettings = (await unitOfWork.GetRepository<Settings>().GetAllAsync()).FirstOrDefault();

            if (emailSettings == null)
            {
                throw new InvalidOperationException("لم يتم العثور على إعدادات البريد الإلكتروني.");
            }

            using (var smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password)
            })
            {
                using (var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSettings.FromEmail, emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    mailMessage.To.Add(toEmail);
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
        }
    }
}