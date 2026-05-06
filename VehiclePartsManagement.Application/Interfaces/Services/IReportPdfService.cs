using VehiclePartsManagement.Application.DTOs.Report;

namespace VehiclePartsManagement.Application.Interfaces.Services;

/// <summary>
/// Generates PDF exports from report DTO payloads.
/// </summary>
public interface IReportPdfService
{
    /// <summary>
    /// Generates a PDF document for the sales revenue report.
    /// </summary>
    Task<byte[]> GenerateSalesRevenueReportPdfAsync(SalesRevenueReportResponseDto report);

    /// <summary>
    /// Generates a PDF document for the purchase expense report.
    /// </summary>
    Task<byte[]> GeneratePurchaseExpenseReportPdfAsync(PurchaseExpenseReportResponseDto report);

    /// <summary>
    /// Generates a PDF document for the profit and loss report.
    /// </summary>
    Task<byte[]> GenerateProfitLossReportPdfAsync(ProfitLossReportResponseDto report);

    /// <summary>
    /// Generates a PDF document for the payment collection report.
    /// </summary>
    Task<byte[]> GeneratePaymentCollectionReportPdfAsync(PaymentCollectionReportResponseDto report);

    /// <summary>
    /// Generates a PDF document for the receivable report.
    /// </summary>
    Task<byte[]> GenerateReceivableReportPdfAsync(ReceivableReportResponseDto report);
}
