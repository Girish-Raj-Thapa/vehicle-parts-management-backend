namespace VehiclePartsManagement.Application.DTOs.Report;

public class DiscountPeriodItemDto
{
    public string Period { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public int DiscountedInvoiceCount { get; set; }
}