using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.DTOs.Report;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/report-pdf")]
public class ReportPdfController(
    IReportService reportService,
    IReportPdfService reportPdfService) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/sales-revenue")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetSalesRevenuePdf([FromQuery] ReportQueryDto query)
    {
        var result = await reportService.GetSalesRevenueReportAsync(query);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        if (result.Data is not SalesRevenueReportResponseDto reportData)
        {
            return BadRequest(result);
        }

        var pdfBytes = await reportPdfService.GenerateSalesRevenueReportPdfAsync(reportData);
        var fileName = $"sales-revenue-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/purchase-expense")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetPurchaseExpensePdf([FromQuery] ReportQueryDto query)
    {
        var result = await reportService.GetPurchaseExpenseReportAsync(query);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        if (result.Data is not PurchaseExpenseReportResponseDto reportData)
        {
            return BadRequest(result);
        }

        var pdfBytes = await reportPdfService.GeneratePurchaseExpenseReportPdfAsync(reportData);
        var fileName = $"purchase-expense-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/profit-loss")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetProfitLossPdf([FromQuery] ReportQueryDto query)
    {
        var result = await reportService.GetProfitLossReportAsync(query);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        if (result.Data is not ProfitLossReportResponseDto reportData)
        {
            return BadRequest(result);
        }

        var pdfBytes = await reportPdfService.GenerateProfitLossReportPdfAsync(reportData);
        var fileName = $"profit-loss-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
