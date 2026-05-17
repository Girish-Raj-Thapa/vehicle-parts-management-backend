using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Report;

namespace VehiclePartsManagement.Application.Interfaces.Services;

/// <summary>
/// Exposes business-level operations for generating financial reports.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generates the sales revenue report for the requested filter range.
    /// </summary>
    Task<ApiResponse<SalesRevenueReportResponseDto>> GetSalesRevenueReportAsync(ReportQueryDto query);

    /// <summary>
    /// Generates the purchase expense report for the requested filter range.
    /// </summary>
    Task<ApiResponse<PurchaseExpenseReportResponseDto>> GetPurchaseExpenseReportAsync(ReportQueryDto query);

    /// <summary>
    /// Generates the payment collection report for the requested filter range.
    /// </summary>
    Task<ApiResponse<PaymentCollectionReportResponseDto>> GetPaymentCollectionReportAsync(ReportQueryDto query);

    /// <summary>
    /// Generates the receivable and overdue credit report.
    /// </summary>
    Task<ApiResponse<ReceivableReportResponseDto>> GetReceivableReportAsync(ReportQueryDto query);

    /// <summary>
    /// Generates the estimated profit and loss report.
    /// </summary>
    Task<ApiResponse<ProfitLossReportResponseDto>> GetProfitLossReportAsync(ReportQueryDto query);

    /// <summary>
    /// Generates the estimated daily cash flow report.
    /// </summary>
    Task<ApiResponse<DailyCashFlowReportResponseDto>> GetDailyCashFlowReportAsync(ReportQueryDto query);

    /// <summary>
    /// Generates the discount utilization and impact report.
    /// </summary>
    Task<ApiResponse<DiscountReportResponseDto>> GetDiscountReportAsync(ReportQueryDto query);

    /// <summary>
    /// Generates the admin dashboard financial summary snapshot.
    /// </summary>
    Task<ApiResponse<DashboardSummaryResponseDto>> GetDashboardSummaryAsync(ReportQueryDto query);
    Task<ApiResponse<IEnumerable<RegularCustomerReportDto>>> GetRegularCustomersAsync();
    Task<ApiResponse<IEnumerable<TopSpenderReportDto>>> GetTopSpendersAsync();
    Task<ApiResponse<IEnumerable<PendingCreditReportDto>>> GetPendingCreditsAsync();
}
