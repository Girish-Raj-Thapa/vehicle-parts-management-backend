namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
    Task SendStaffRegistrationEmailAsync(
        string toEmail,
        string fullName,
        string username,
        string password);
}
