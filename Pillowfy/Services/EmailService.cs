using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Pillowfy.Services.EmailService  // ✅ namespace corrigé
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class EmailService : IEmailService  // ✅ hérite de l'interface, pas d'elle-même
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var settings = _config.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                settings["SenderName"],
                settings["SenderEmail"]
            ));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                settings["SmtpHost"],
                int.Parse(settings["SmtpPort"]!),
                SecureSocketOptions.StartTls
            );
            await client.AuthenticateAsync(
                settings["SenderEmail"],
                settings["Password"]
            );
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}