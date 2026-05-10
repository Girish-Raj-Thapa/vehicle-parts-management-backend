using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Customer;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "Admin,Staff")]
public class CustomersController(IUserService userService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await userService.GetCustomerDetailsAsync(id);
        return result.Success
            ? Ok(result)
            : NotFound(ApiResponse<CustomerDetailsDto>.FailureResponse(result.Message, result.Errors));
    }


    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(string id)
    {
        var result = await userService.GetCustomerHistoryAsync(id);
        return result.Success
            ? Ok(result)
            : NotFound(ApiResponse<CustomerHistoryDto>.FailureResponse(result.Message, result.Errors));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] CustomerSearchQueryDto dto)
    {
        var result = await userService.SearchCustomersAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/loyalty")]
    public async Task<IActionResult> GetLoyalty(string id)
    {
        var result = await userService.GetCustomerLoyaltyAsync(id);
        return result.Success
            ? Ok(result)
            : NotFound(ApiResponse<CustomerLoyaltyDto>.FailureResponse(result.Message, result.Errors));
    }
}
