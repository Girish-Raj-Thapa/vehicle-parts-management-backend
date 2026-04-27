namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerSearchItemDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<CustomerSearchVehicleDto> Vehicles { get; set; } = [];
}
