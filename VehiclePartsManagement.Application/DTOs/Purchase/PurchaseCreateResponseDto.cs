namespace VehiclePartsManagement.Application.DTOs.Purchase;

public class PurchaseCreateResponseDto
{
    public int PurchaseId { get; set; }
    public string PurchaseInvoiceNo { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PurchaseStatus { get; set; } = string.Empty;
}
