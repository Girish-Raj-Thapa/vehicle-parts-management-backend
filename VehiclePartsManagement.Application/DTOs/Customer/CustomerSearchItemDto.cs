namespace VehiclePartsManagement.Application.DTOs.Customer;

/// <summary>
/// Customer row returned by customer search endpoint.
/// </summary>
public class CustomerSearchItemDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    // Includes all customer vehicles so staff can disambiguate similar names/phones.
    public List<CustomerSearchVehicleDto> Vehicles { get; set; } = [];
}
