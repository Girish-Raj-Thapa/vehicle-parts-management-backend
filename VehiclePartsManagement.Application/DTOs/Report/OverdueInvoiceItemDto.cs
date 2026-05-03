namespace VehiclePartsManagement.Application.DTOs.Report;

public class OverdueInvoiceItemDto
{
    public int SaleId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal BalanceDue { get; set; }
    public int DaysOverdue { get; set; }
}