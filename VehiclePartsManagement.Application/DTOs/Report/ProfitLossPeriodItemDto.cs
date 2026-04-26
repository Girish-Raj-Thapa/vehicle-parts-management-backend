namespace VehiclePartsManagement.Application.DTOs.Report;

public class ProfitLossPeriodItemDto
{
    public string Period { get; set; } = string.Empty;
    public decimal SalesRevenue { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal ServiceLaborIncome { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal EstimatedGrossProfit { get; set; }
    public decimal EstimatedNetProfit { get; set; }
}