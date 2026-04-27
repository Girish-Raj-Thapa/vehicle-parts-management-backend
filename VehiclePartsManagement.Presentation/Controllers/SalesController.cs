using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController(ISaleService saleService) : ControllerBase
{
    [HttpPost("{saleId}/send-email")]
    public async Task<IActionResult> SendEmail(int saleId)
    {
        var response = await saleService.SendInvoiceEmailAsync(saleId);
        return Ok(response);
    }
}
