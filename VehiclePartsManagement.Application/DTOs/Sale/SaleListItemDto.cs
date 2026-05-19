namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// List row payload for paged sales responses.
/// </summary>
public class SaleListItemDto
{
    public int SaleId { get; set; }
    public string SaleInvoiceNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateOnly SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public string SaleChannel { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateOnly? DueDate { get; set; }
}
