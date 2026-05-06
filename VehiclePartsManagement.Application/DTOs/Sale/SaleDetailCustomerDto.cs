namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// Customer section in a sale detail response.
/// </summary>
public class SaleDetailCustomerDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
