namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerDetailsDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public bool IsActive { get; set; }
    public List<CustomerVehicleInfoDto> Vehicles { get; set; } = [];
}
