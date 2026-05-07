namespace VehiclePartsManagement.Application.DTOs.Vehicle;

/// <summary>
/// Compact vehicle row for paged vehicle list responses.
/// </summary>
public class VehicleListItemDto
{
    public int VehicleId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public int? ManufactureYear { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int CurrentMileage { get; set; }
    public string? PrimaryImagePath { get; set; }
}
