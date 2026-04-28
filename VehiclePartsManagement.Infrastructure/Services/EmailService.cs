using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Infrastructure.Config;

namespace VehiclePartsManagement.Infrastructure.Services;

public class EmailService(IOptions<EmailOptions> emailOptions) : IEmailService
{
    private readonly EmailOptions _options = emailOptions.Value;

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            
            // Connect to the server
            await smtp.ConnectAsync(_options.SmtpServer, _options.SmtpPort, _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
            
            // Authenticate if username/password are provided
            if (!string.IsNullOrEmpty(_options.Username) && !string.IsNullOrEmpty(_options.Password))
            {
                await smtp.AuthenticateAsync(_options.Username, _options.Password);
            }

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            // Detailed logging for troubleshooting
            Console.WriteLine("==================================================");
            Console.WriteLine($"[Email Error] Failed to send email to {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
            }
            Console.WriteLine("==================================================");
            return false;
        }
    }
}
