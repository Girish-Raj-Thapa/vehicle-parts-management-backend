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
}
