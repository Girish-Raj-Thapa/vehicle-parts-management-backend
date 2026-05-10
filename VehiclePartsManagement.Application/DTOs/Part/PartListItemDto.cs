namespace VehiclePartsManagement.Application.DTOs.Part;

public class PartListItemDto
{
    public int PartId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public int CurrentStock { get; set; }
    public bool IsInStock { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsActive { get; set; }
    public List<string> CompatibleModels { get; set; } = [];
}
