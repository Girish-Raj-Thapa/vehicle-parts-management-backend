using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.DTOs.Report;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(
    IReportsService reportsService) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/sales-revenue")]
    public async Task<IActionResult> GetSalesRevenue([FromQuery] ReportQueryDto query)
    {
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

        var result = await reportsService.GetSalesRevenueReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("admin/purchase-expense")]
    public async Task<IActionResult> GetPurchaseExpense([FromQuery] ReportQueryDto query)
    {
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

        var result = await reportsService.GetPurchaseExpenseReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/payment-collection")]
    public async Task<IActionResult> GetPaymentCollection([FromQuery] ReportQueryDto query)
    {
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

        var result = await reportsService.GetPaymentCollectionReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/receivables")]
    public async Task<IActionResult> GetReceivables([FromQuery] ReportQueryDto query)
    {
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

        var result = await reportsService.GetReceivableReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/profit-loss")]
    public async Task<IActionResult> GetProfitLoss([FromQuery] ReportQueryDto query)
    {
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

        var result = await reportsService.GetProfitLossReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/daily-cash-flow")]
    public async Task<IActionResult> GetDailyCashFlow([FromQuery] ReportQueryDto query)
    {
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

        var result = await reportsService.GetDailyCashFlowReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/discounts")]
    public async Task<IActionResult> GetDiscounts([FromQuery] ReportQueryDto query)
    {
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

        var result = await reportsService.GetDiscountReportAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/summary")]
    public async Task<IActionResult> GetDashboardSummary([FromQuery] ReportQueryDto query)
    {
        if (!Request.Query.ContainsKey("groupBy"))
        {
            query.GroupBy = string.Empty;
        }

        var result = await reportsService.GetDashboardSummaryAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("customers/regulars")]
    public async Task<IActionResult> GetRegularCustomers()
    {
        var response = await reportsService.GetRegularCustomersAsync();
        return Ok(response);
    }

    [HttpGet("customers/top-spenders")]
    public async Task<IActionResult> GetTopSpenders()
    {
        var response = await reportsService.GetTopSpendersAsync();
        return Ok(response);
    }

    [HttpGet("customers/pending-credits")]
    public async Task<IActionResult> GetPendingCredits()
    {
        var response = await reportsService.GetPendingCreditsAsync();
        return Ok(response);
    }
}
