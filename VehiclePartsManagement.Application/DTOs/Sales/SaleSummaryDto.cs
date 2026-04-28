namespace VehiclePartsManagement.Application.DTOs.Sales;

public class SaleSummaryDto
{
    public int SaleId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public DateTime SaleDate { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}
