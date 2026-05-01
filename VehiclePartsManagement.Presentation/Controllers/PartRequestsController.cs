using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.PartRequest;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/part-requests")]
[Authorize]
public class PartRequestsController(IPartRequestService partRequestService) : ControllerBase
{
    /// <summary>
    /// Creates a new part request for the authenticated customer.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create([FromBody] CreatePartRequestDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await partRequestService.CreateAsync(dto, userId);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }


    /// <summary>
    /// Returns paged part requests; customer scope is restricted to own requests.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetPaged([FromQuery] PartRequestQueryDto query)
    {
        var isCustomer = User.IsInRole("Customer");
        var userId = isCustomer || query.Mine ? GetCurrentUserId() : null;
        if ((isCustomer || query.Mine) && string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        // Customer listing is always scoped to own records regardless of incoming mine value.
        if (isCustomer)
        {
            query.Mine = true;
        }

        var result = await partRequestService.GetPagedAsync(query, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    /// <summary>
    /// Returns one part request; customer scope is restricted to own records.
    /// </summary>
    [HttpGet("{partRequestId:int}")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetById(int partRequestId)
    {
        var scopedUserId = User.IsInRole("Customer") ? GetCurrentUserId() : null;
        if (User.IsInRole("Customer") && string.IsNullOrWhiteSpace(scopedUserId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await partRequestService.GetByIdAsync(partRequestId, scopedUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }


    /// <summary>
    /// Updates part-request status and fulfillment details by staff/admin.
    /// </summary>
    [HttpPut("{partRequestId:int}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int partRequestId, [FromBody] UpdatePartRequestDto dto)
    {
        var result = await partRequestService.UpdateAsync(partRequestId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    private string? GetCurrentUserId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}
