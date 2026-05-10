namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// Vehicle section in a sale detail response.
/// </summary>
public class SaleDetailVehicleDto
{
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
}
