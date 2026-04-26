namespace VehiclePartsManagement.Application.DTOs.Report;

public class SalesRevenueReportResponseDto
{
    public ReportMetaDto Meta { get; set; } = new();
    public decimal TotalSalesAmount { get; set; }
    public int TotalInvoices { get; set; }
    public decimal CashSalesAmount { get; set; }
    public decimal CreditSalesAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public List<SalesRevenuePeriodItemDto> Collections { get; set; } = [];
}