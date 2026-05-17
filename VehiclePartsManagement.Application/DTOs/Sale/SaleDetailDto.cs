namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// Detailed sale payload for single-sale endpoint.
/// </summary>
public class SaleDetailDto
{
    public int SaleId { get; set; }
    public string SaleInvoiceNo { get; set; } = string.Empty;
    public DateOnly SaleDate { get; set; }
    public SaleDetailCustomerDto Customer { get; set; } = new();
    public SaleDetailVehicleDto Vehicle { get; set; } = new();
    public List<SaleDetailItemDto> Items { get; set; } = [];
    public decimal SubTotal { get; set; }
    public decimal LoyaltyDiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateOnly? DueDate { get; set; }
}
