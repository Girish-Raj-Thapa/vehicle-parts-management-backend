namespace VehiclePartsManagement.Application.DTOs.Vehicle;

/// <summary>
/// Full vehicle detail payload for the single vehicle detail endpoint.
/// </summary>
public class VehicleDetailDto
{
    public int VehicleId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerPhone { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public int? ManufactureYear { get; set; }
    public string EngineNumber { get; set; } = string.Empty;
    public string ChassisNumber { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int CurrentMileage { get; set; }
    public DateTime VehicleRegisteredAt { get; set; }
    public List<string> ImagePaths { get; set; } = [];
    public int TotalServiceRecords { get; set; }
    public int TotalSales { get; set; }
    public int TotalPartRequests { get; set; }
}
