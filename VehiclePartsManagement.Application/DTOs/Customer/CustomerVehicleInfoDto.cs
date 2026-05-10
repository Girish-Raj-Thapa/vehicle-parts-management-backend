namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerVehicleInfoDto
{
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public int? ManufactureYear { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int CurrentMileage { get; set; }
}
