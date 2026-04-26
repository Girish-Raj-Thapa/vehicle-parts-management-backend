namespace VehiclePartsManagement.Application.DTOs.Report;

public class ProfitLossReportResponseDto
{
    public ReportMetaDto Meta { get; set; } = new();
    public decimal TotalSalesRevenue { get; set; }
    public decimal TotalPurchaseCost { get; set; }
    public decimal ServiceLaborIncome { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal EstimatedGrossProfit { get; set; }
    public decimal EstimatedNetProfit { get; set; }
    public List<ProfitLossPeriodItemDto> Collections { get; set; } = [];
}