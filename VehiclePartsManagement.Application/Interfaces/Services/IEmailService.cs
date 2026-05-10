namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendStaffRegistrationEmailAsync(
        string toEmail,
        string fullName,
        string username,
        string password);
}
