namespace VehiclePartsManagement.Application.DTOs.Purchase;

public class PurchaseDetailDto
{
    public int PurchaseId { get; set; }
    public string PurchaseInvoiceNo { get; set; } = string.Empty;
    public DateOnly PurchaseDate { get; set; }
    public PurchaseVendorDto Vendor { get; set; } = new();
    public PurchaseCreatedByDto CreatedBy { get; set; } = new();
    public List<PurchaseItemDto> Items { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public string PurchaseStatus { get; set; } = string.Empty;
}
