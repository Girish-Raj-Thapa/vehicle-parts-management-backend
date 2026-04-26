namespace VehiclePartsManagement.Application.DTOs.Report;

public class DashboardSummaryResponseDto
{
    public ReportMetaDto Meta { get; set; } = new();
    public decimal TodaySales { get; set; }
    public decimal MonthSales { get; set; }
    public decimal MonthPurchases { get; set; }
    public decimal MonthPaymentsReceived { get; set; }
    public decimal PendingCreditAmount { get; set; }
    public decimal EstimatedMonthProfit { get; set; }
    public LowStockFinancialRiskItemDto LowStockFinancialRisk { get; set; } = new();
    public List<PendingReceivableItemDto> TopPendingReceivables { get; set; } = [];
}