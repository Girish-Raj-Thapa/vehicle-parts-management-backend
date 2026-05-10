using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Infrastructure.Config;

namespace VehiclePartsManagement.Infrastructure.Services;

public class EmailService(IOptions<EmailOptions> options) : IEmailService
{
    private readonly EmailOptions _options = options.Value;


    public async Task SendStaffRegistrationEmailAsync(
        string toEmail,
        string fullName,
        string username,
        string password)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Your staff account has been registered";

        var bodyBuilder = new BodyBuilder
        {
            TextBody = $"""
                Hello {fullName},

                Your staff account has been registered successfully.

                Username: {username}
                Password: {password}

                Please log in and change your password if the application provides that option.

                Regards,
                Vehicle Parts Management
                """,
            HtmlBody = $"""
                <p>Hello {fullName},</p>
                <p>Your staff account has been registered successfully.</p>
                <p>
                    <strong>Username:</strong> {username}<br />
                    <strong>Password:</strong> {password}
                </p>
                <p>Please log in and change your password if the application provides that option.</p>
                <p>Regards,<br />Vehicle Parts Management</p>
                """
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_options.Username, _options.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
