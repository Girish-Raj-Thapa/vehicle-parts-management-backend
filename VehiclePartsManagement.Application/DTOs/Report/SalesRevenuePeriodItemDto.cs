namespace VehiclePartsManagement.Application.DTOs.Report;

public class SalesRevenuePeriodItemDto
{
    public string Period { get; set; } = string.Empty;
    public decimal SalesAmount { get; set; }
    public int InvoiceCount { get; set; }
    public decimal DiscountAmount { get; set; }
}