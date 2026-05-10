using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController(ISaleService saleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllSales()
    {
        var response = await saleService.GetAllSalesAsync();
        return Ok(response);
    }

    [HttpPost("{saleId}/send-email")]
    public async Task<IActionResult> SendEmail(int saleId)
    {
        var response = await saleService.SendInvoiceEmailAsync(saleId);
        return Ok(response);
    }
}

