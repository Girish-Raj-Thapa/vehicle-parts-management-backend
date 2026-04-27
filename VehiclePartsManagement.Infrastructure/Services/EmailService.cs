using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        // Mock implementation: Simulate email sending
        await Task.Delay(100); 
        Console.WriteLine($"[Mock Email] To: {to}, Subject: {subject}");
        return true;
    }
}
