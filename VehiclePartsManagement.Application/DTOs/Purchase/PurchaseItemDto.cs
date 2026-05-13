namespace VehiclePartsManagement.Application.DTOs.Purchase;

public class PurchaseItemDto
{
    public int PurchaseItemId { get; set; }
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int QuantityPurchased { get; set; }
    public decimal UnitPriceAtPurchase { get; set; }
    public decimal PurchaseLineTotal { get; set; }
}
