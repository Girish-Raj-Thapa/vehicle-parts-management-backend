namespace VehiclePartsManagement.Application.DTOs.Part;

public class PartImageDto
{
    public int PartImageId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}
