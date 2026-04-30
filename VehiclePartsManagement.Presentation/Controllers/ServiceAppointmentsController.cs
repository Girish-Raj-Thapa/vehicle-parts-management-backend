using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.ServiceAppointment;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/service-appointments")]
[Authorize]
public class ServiceAppointmentsController(IServiceAppointmentService serviceAppointmentService) : ControllerBase
{
    /// <summary>
    /// Books a new appointment for the authenticated customer.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create([FromBody] CreateServiceAppointmentDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await serviceAppointmentService.CreateAsync(dto, userId);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }


    /// <summary>
    /// Returns paged appointments with optional mine/status filters.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetPaged([FromQuery] ServiceAppointmentQueryDto query)
    {
        var isCustomer = User.IsInRole("Customer");
        var userId = isCustomer || query.Mine ? GetCurrentUserId() : null;
        if ((isCustomer || query.Mine) && string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await serviceAppointmentService.GetPagedAsync(query, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    /// <summary>
    /// Returns appointment detail. Customer access is scoped to own records.
    /// </summary>
    [HttpGet("{appointmentId:int}")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetById(int appointmentId)
    {
        var scopedUserId = User.IsInRole("Customer") ? GetCurrentUserId() : null;
        if (User.IsInRole("Customer") && string.IsNullOrWhiteSpace(scopedUserId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await serviceAppointmentService.GetByIdAsync(appointmentId, scopedUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }


    private string? GetCurrentUserId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}
