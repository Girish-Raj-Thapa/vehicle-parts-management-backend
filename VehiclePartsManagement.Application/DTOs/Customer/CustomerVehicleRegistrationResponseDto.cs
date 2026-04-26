namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerVehicleRegistrationResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string? TemporaryPassword { get; set; }
}
