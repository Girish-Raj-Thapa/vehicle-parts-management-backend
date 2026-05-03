namespace VehiclePartsManagement.Application.DTOs.Report;

public class ReceivableCustomerBalanceItemDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal PendingBalance { get; set; }
    public decimal OverdueBalance { get; set; }
    public int InvoiceCount { get; set; }
}