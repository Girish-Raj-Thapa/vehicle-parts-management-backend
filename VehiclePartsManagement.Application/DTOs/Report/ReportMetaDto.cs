namespace VehiclePartsManagement.Application.DTOs.Report;

/// <summary>
/// Represents metadata common to all generated report responses.
/// </summary>
public class ReportMetaDto
{
    /// <summary>
    /// Human-readable report title shown in UI and exports.
    /// </summary>
    public string ReportTitle { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the report was generated.
    /// </summary>
    public DateTime GeneratedAtUtc { get; set; }

    /// <summary>
    /// Effective UTC date range used for the report query.
    /// </summary>
    public ReportDateRangeDto DateRange { get; set; } = new();

    /// <summary>
    /// Aggregation granularity used for period collections.
    /// </summary>
    public string GroupBy { get; set; } = "day";

    /// <summary>
    /// Indicates whether cancelled records were included.
    /// </summary>
    public bool IncludeCancelled { get; set; }

    /// <summary>
    /// Optional customer filter applied to the report.
    /// </summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// Optional vendor filter applied to the report.
    /// </summary>
    public int? VendorId { get; set; }

    /// <summary>
    /// Documents assumptions used in calculations for transparency.
    /// </summary>
    public string CalculationNote { get; set; } = string.Empty;
}
