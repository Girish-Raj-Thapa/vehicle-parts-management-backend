namespace VehiclePartsManagement.Application.DTOs.Purchase;

public class PurchaseListItemDto
{
    public int PurchaseId { get; set; }
    public string PurchaseInvoiceNo { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public DateOnly PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string PurchaseStatus { get; set; } = string.Empty;
}
