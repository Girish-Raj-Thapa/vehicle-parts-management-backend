namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerSearchVehicleDto
{
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
}
