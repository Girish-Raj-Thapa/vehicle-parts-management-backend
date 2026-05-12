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
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

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
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

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
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

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

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/payment-collection")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetPaymentCollectionPdf([FromQuery] ReportQueryDto query)
    {
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

        var result = await reportService.GetPaymentCollectionReportAsync(query);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        if (result.Data is not PaymentCollectionReportResponseDto reportData)
        {
            return BadRequest(result);
        }

        var pdfBytes = await reportPdfService.GeneratePaymentCollectionReportPdfAsync(reportData);
        var fileName = $"payment-collection-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/receivables")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetReceivablesPdf([FromQuery] ReportQueryDto query)
    {
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

        var result = await reportService.GetReceivableReportAsync(query);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        if (result.Data is not ReceivableReportResponseDto reportData)
        {
            return BadRequest(result);
        }

        var pdfBytes = await reportPdfService.GenerateReceivableReportPdfAsync(reportData);
        var fileName = $"receivables-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/daily-cash-flow")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetDailyCashFlowPdf([FromQuery] ReportQueryDto query)
    {
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

        var result = await reportService.GetDailyCashFlowReportAsync(query);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        if (result.Data is not DailyCashFlowReportResponseDto reportData)
        {
            return BadRequest(result);
        }

        var pdfBytes = await reportPdfService.GenerateDailyCashFlowReportPdfAsync(reportData);
        var fileName = $"daily-cash-flow-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/discounts")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetDiscountsPdf([FromQuery] ReportQueryDto query)
    {
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

        var result = await reportService.GetDiscountReportAsync(query);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        if (result.Data is not DiscountReportResponseDto reportData)
        {
            return BadRequest(result);
        }

        var pdfBytes = await reportPdfService.GenerateDiscountReportPdfAsync(reportData);
        var fileName = $"discount-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/summary")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetDashboardSummaryPdf([FromQuery] ReportQueryDto query)
    {
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

        var result = await reportService.GetDashboardSummaryAsync(query);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        if (result.Data is not DashboardSummaryResponseDto reportData)
        {
            return BadRequest(result);
        }

        var pdfBytes = await reportPdfService.GenerateDashboardSummaryPdfAsync(reportData);
        var fileName = $"dashboard-summary-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
