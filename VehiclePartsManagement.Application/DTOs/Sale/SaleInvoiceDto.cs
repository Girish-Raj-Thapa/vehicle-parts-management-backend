namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// Invoice-ready payload for a specific sale.
/// </summary>
public class SaleInvoiceDto
{
    public string InvoiceNo { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string Vehicle { get; set; } = string.Empty;
    public List<SaleInvoiceItemDto> Items { get; set; } = [];
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
}
