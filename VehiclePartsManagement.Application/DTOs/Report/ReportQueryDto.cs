using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Report;

/// <summary>
/// Carries common query parameters used across report endpoints.
/// </summary>
public class ReportQueryDto
{
    /// <summary>
    /// Inclusive report start date/time in UTC.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Inclusive report end date/time in UTC.
    /// </summary>
    public DateTime? To { get; set; }

    /// <summary>
    /// Controls aggregation level: day, month, or year.
    /// </summary>
    [RegularExpression("^(day|month|year)$", ErrorMessage = "groupBy must be one of: day, month, year")]
    public string GroupBy { get; set; } = "day";

    /// <summary>
    /// When true, cancelled documents are also included in calculations.
    /// </summary>
    public bool IncludeCancelled { get; set; } = false;

    /// <summary>
    /// Optional customer filter for customer-specific report views.
    /// </summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// Optional vendor filter for vendor-specific purchase views.
    /// </summary>
    public int? VendorId { get; set; }

    /// <summary>
    /// Overdue threshold in days for receivable calculations.
    /// </summary>
    public int OverdueDays { get; set; } = 30;

    [Range(1, int.MaxValue, ErrorMessage = "page must be greater than or equal to 1")]
    public int Page { get; set; } = 1;

    [Range(1, 10, ErrorMessage = "pageSize must be between 1 and 10")]
    public int PageSize { get; set; } = 10;

    [Range(1, int.MaxValue, ErrorMessage = "vendorPage must be greater than or equal to 1")]
    public int? VendorPage { get; set; }

    [Range(1, 10, ErrorMessage = "vendorPageSize must be between 1 and 10")]
    public int? VendorPageSize { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "partPage must be greater than or equal to 1")]
    public int? PartPage { get; set; }

    [Range(1, 10, ErrorMessage = "partPageSize must be between 1 and 10")]
    public int? PartPageSize { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "collectionPage must be greater than or equal to 1")]
    public int? CollectionPage { get; set; }

    [Range(1, 10, ErrorMessage = "collectionPageSize must be between 1 and 10")]
    public int? CollectionPageSize { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "methodPage must be greater than or equal to 1")]
    public int? MethodPage { get; set; }

    [Range(1, 10, ErrorMessage = "methodPageSize must be between 1 and 10")]
    public int? MethodPageSize { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "customerBalancePage must be greater than or equal to 1")]
    public int? CustomerBalancePage { get; set; }

    [Range(1, 10, ErrorMessage = "customerBalancePageSize must be between 1 and 10")]
    public int? CustomerBalancePageSize { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "overdueInvoicePage must be greater than or equal to 1")]
    public int? OverdueInvoicePage { get; set; }

    [Range(1, 10, ErrorMessage = "overdueInvoicePageSize must be between 1 and 10")]
    public int? OverdueInvoicePageSize { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "dailyFlowPage must be greater than or equal to 1")]
    public int? DailyFlowPage { get; set; }

    [Range(1, 10, ErrorMessage = "dailyFlowPageSize must be between 1 and 10")]
    public int? DailyFlowPageSize { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "pendingReceivablePage must be greater than or equal to 1")]
    public int? PendingReceivablePage { get; set; }

    [Range(1, 10, ErrorMessage = "pendingReceivablePageSize must be between 1 and 10")]
    public int? PendingReceivablePageSize { get; set; }
}
