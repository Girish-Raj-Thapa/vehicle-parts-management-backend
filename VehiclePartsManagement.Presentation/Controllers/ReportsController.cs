using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.DTOs.Report;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(
    IReportService reportService) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/sales-revenue")]
    public async Task<IActionResult> GetSalesRevenue([FromQuery] ReportQueryDto query)
    {
        var result = await reportService.GetSalesRevenueReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("admin/purchase-expense")]
    public async Task<IActionResult> GetPurchaseExpense([FromQuery] ReportQueryDto query)
    {
        var result = await reportService.GetPurchaseExpenseReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/payment-collection")]
    public async Task<IActionResult> GetPaymentCollection([FromQuery] ReportQueryDto query)
    {
        var result = await reportService.GetPaymentCollectionReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/receivables")]
    public async Task<IActionResult> GetReceivables([FromQuery] ReportQueryDto query)
    {
        var result = await reportService.GetReceivableReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/profit-loss")]
    public async Task<IActionResult> GetProfitLoss([FromQuery] ReportQueryDto query)
    {
        var result = await reportService.GetProfitLossReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/daily-cash-flow")]
    public async Task<IActionResult> GetDailyCashFlow([FromQuery] ReportQueryDto query)
    {
        var result = await reportService.GetDailyCashFlowReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/discounts")]
    public async Task<IActionResult> GetDiscounts([FromQuery] ReportQueryDto query)
    {
        var result = await reportService.GetDiscountReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var result = await reportService.GetDashboardSummaryAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("customers/regulars")]
    public async Task<IActionResult> GetRegularCustomers()
    {
        var response = await reportService.GetRegularCustomersAsync();
        return Ok(response);
    }

    [HttpGet("customers/top-spenders")]
    public async Task<IActionResult> GetTopSpenders()
    {
        var response = await reportService.GetTopSpendersAsync();
        return Ok(response);
    }

    [HttpGet("customers/pending-credits")]
    public async Task<IActionResult> GetPendingCredits()
    {
        var response = await reportService.GetPendingCreditsAsync();
        return Ok(response);
    }
}
