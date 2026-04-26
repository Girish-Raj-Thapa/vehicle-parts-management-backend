namespace VehiclePartsManagement.Application.DTOs.Report;

public class PurchaseExpensePartItemDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int QuantityPurchased { get; set; }
    public decimal PurchaseAmount { get; set; }
}