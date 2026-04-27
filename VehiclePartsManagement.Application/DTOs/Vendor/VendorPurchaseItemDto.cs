namespace VehiclePartsManagement.Application.DTOs.Vendor;

public class VendorPurchaseItemDto
{
    public int PurchaseId { get; set; }
    public string PurchaseInvoiceNo { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public string PurchaseStatus { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}