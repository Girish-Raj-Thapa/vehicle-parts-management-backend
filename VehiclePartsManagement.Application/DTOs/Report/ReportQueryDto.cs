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
}
