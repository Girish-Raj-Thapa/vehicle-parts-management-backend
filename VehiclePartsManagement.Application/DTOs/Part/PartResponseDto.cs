namespace VehiclePartsManagement.Application.DTOs.Part;

public class PartResponseDto
{
    public int PartId { get; set; }
    public int PartCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public string PartDescription { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public int CurrentStock { get; set; }
    public int LowStockThreshold { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsInStock { get; set; }
    public bool IsActive { get; set; }
}
