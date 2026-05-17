namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// Response payload returned after successful sale creation.
/// </summary>
public class SaleCreateResponseDto
{
    public int SaleId { get; set; }
    public string SaleInvoiceNo { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal LoyaltyDiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}
