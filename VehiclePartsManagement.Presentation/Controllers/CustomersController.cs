using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.DTOs.Customer;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController(IUserService userService) : ControllerBase
{
    [HttpGet("search")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> Search([FromQuery] CustomerSearchQueryDto dto)
    {
        var result = await userService.SearchCustomersAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
