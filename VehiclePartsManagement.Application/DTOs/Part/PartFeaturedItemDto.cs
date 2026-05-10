namespace VehiclePartsManagement.Application.DTOs.Part;

public class PartFeaturedItemDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public int CurrentStock { get; set; }
    public string Tag { get; set; } = "HOT";
    public List<string> CompatibleModels { get; set; } = [];
}
