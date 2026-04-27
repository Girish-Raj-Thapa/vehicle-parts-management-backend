using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(IReportService reportService) : ControllerBase
{
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
