using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.User;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? role = null,
        [FromQuery] string? search = null)
    {
        if (IsStaffOnly())
        {
            role = "Customer";
        }

        var result = await userService.GetPagedAsync(page, pageSize, role, search);
        return Ok(result);
    }


    [HttpGet("{userId}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetById(string userId)
    {
        var result = await userService.GetByIdAsync(userId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return IsStaffAllowedToManageUser(result.Data as UserDetailDto)
            ? Ok(result)
            : Forbid();
    }


    [HttpPost("staff")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffDto dto)
    {
        var result = await userService.CreateStaffAsync(dto);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }


    [HttpPut("{userId}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(string userId, [FromBody] UpdateUserDto dto)
    {
        if (!await IsStaffAllowedToManageUserAsync(userId))
        {
            return Forbid();
        }

        var result = await userService.UpdateAsync(userId, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }


    [HttpPatch("{userId}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(string userId, [FromBody] UpdateUserStatusDto dto)
    {
        if (!await IsStaffAllowedToManageUserAsync(userId))
        {
            return Forbid();
        }

        var result = await userService.UpdateStatusAsync(userId, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }


    [HttpPut("{userId}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRoles(string userId, [FromBody] UpdateUserRolesDto dto)
    {
        var result = await userService.UpdateRolesAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await userService.GetProfileAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }


    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await userService.UpdateProfileAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    private string? GetCurrentUserId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);


    private bool IsStaffOnly() =>
        User.IsInRole("Staff") && !User.IsInRole("Admin");


    private bool IsStaffAllowedToManageUser(UserDetailDto? user)
    {
        if (!IsStaffOnly())
        {
            return true;
        }

        return user?.Roles.Count == 1 &&
               user.Roles.Any(role => string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase));
    }


    private async Task<bool> IsStaffAllowedToManageUserAsync(string userId)
    {
        if (!IsStaffOnly())
        {
            return true;
        }

        var result = await userService.GetByIdAsync(userId);
        return result.Success && IsStaffAllowedToManageUser(result.Data as UserDetailDto);
    }
}
