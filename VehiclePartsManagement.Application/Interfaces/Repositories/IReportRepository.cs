using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Application.DTOs.Report;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

/// <summary>
/// Provides read-only data access queries required for financial reporting.
/// </summary>
public interface IReportRepository
{
    /// <summary>
    /// Returns sales records with related details within the date range.
    /// </summary>
    Task<List<Sale>> GetSalesWithDetailsAsync(DateTime fromUtc, DateTime toUtc, bool includeCancelled);

    /// <summary>
    /// Returns sales records focused on balance and receivable calculations.
    /// </summary>
    Task<List<Sale>> GetSalesWithBalancesAsync(DateTime fromUtc, DateTime toUtc, bool includeCancelled);

    /// <summary>
    /// Returns purchases with vendor and line-item details within the date range.
    /// </summary>
    Task<List<Purchase>> GetPurchasesWithDetailsAsync(DateTime fromUtc, DateTime toUtc, bool includeCancelled);

    /// <summary>
    /// Returns payments and linked sale information within the date range.
    /// </summary>
    Task<List<Payment>> GetPaymentsWithSaleAsync(DateTime fromUtc, DateTime toUtc);

    /// <summary>
    /// Returns service records used for service income calculations.
    /// </summary>
    Task<List<ServiceRecord>> GetServiceRecordsAsync(DateTime fromUtc, DateTime toUtc);

    /// <summary>
    /// Returns all sales with pending balances.
    /// </summary>
    Task<List<Sale>> GetOutstandingSalesAsync();

    /// <summary>
    /// Returns parts currently at or below low-stock threshold.
    /// </summary>
    Task<List<Part>> GetLowStockPartsAsync();
    Task<IEnumerable<RegularCustomerReportDto>> GetRegularCustomersAsync();
    Task<IEnumerable<TopSpenderReportDto>> GetTopSpendersAsync();
    Task<IEnumerable<PendingCreditReportDto>> GetPendingCreditsAsync();
}
