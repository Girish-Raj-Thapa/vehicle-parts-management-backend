namespace VehiclePartsManagement.Application.DTOs.Customer;

/// <summary>
/// Vehicle details embedded in customer search results.
/// </summary>
public class CustomerSearchVehicleDto
{
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
}
