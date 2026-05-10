namespace VehiclePartsManagement.Application.DTOs.Part;

public class PartCompatibleVehicleDto
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public int ModelId { get; set; }
    public string ModelName { get; set; } = string.Empty;
}
