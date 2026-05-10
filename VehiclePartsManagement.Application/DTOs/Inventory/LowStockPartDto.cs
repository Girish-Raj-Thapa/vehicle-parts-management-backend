namespace VehiclePartsManagement.Application.DTOs.Inventory;

public class LowStockPartDto
{
    public int PartId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int LowStockThreshold { get; set; }
    public string Status { get; set; } = "Low Stock";
}
