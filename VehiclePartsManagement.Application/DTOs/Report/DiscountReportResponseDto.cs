namespace VehiclePartsManagement.Application.DTOs.Report;

public class DiscountReportResponseDto
{
    public ReportMetaDto Meta { get; set; } = new();
    public decimal TotalDiscountGiven { get; set; }
    public int DiscountedInvoiceCount { get; set; }
    public int Loyalty10PercentInvoiceCount { get; set; }
    public decimal ImpactOnRevenuePercent { get; set; }
    public List<DiscountCustomerItemDto> CustomerDiscounts { get; set; } = [];
    public List<DiscountPeriodItemDto> Collections { get; set; } = [];
}