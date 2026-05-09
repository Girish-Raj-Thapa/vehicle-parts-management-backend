namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerPurchaseDetailDto
{
    public int SaleId { get; set; }
    public string SaleInvoiceNo { get; set; } = string.Empty;
    public DateOnly SaleDate { get; set; }
    public string SaleStatus { get; set; } = string.Empty;
    public CustomerHistoryVehicleDto Vehicle { get; set; } = new();
    public List<CustomerPurchaseItemDto> Items { get; set; } = [];
    public decimal SubTotal { get; set; }
    public decimal LoyaltyDiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}
