using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController(IPartService partService) : ControllerBase
{
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var response = await partService.GetLowStockPartsAsync();
        return Ok(response);
    }
}
