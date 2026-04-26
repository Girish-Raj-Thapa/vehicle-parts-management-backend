using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.DTOs.User;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? role = null,
        [FromQuery] string? search = null)
    {
        var result = await userService.GetPagedAsync(page, pageSize, role, search);
        return Ok(result);
    }


    [HttpGet("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(string userId)
    {
        var result = await userService.GetByIdAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string userId, [FromBody] UpdateUserDto dto)
    {
        var result = await userService.UpdateAsync(userId, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }


    [HttpPatch("{userId}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(string userId, [FromBody] UpdateUserStatusDto dto)
    {
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
}
