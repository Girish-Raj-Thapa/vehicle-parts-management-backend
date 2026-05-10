namespace VehiclePartsManagement.Application.DTOs.Sale;

public class SaleResponseDto
{
    public int SaleId { get; set; }
    public string SaleInvoiceNo { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal LoyaltyDiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
