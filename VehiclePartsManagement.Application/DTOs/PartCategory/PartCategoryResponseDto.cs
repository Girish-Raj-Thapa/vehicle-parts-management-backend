namespace VehiclePartsManagement.Application.DTOs.PartCategory;

public class PartCategoryResponseDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryDescription { get; set; } = string.Empty;
}
