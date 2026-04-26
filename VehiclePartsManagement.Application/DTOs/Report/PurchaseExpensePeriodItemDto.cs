namespace VehiclePartsManagement.Application.DTOs.Report;

public class PurchaseExpensePeriodItemDto
{
    public string Period { get; set; } = string.Empty;
    public decimal PurchaseAmount { get; set; }
    public int InvoiceCount { get; set; }
}