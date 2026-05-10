using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.DTOs.Part;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/parts")]
[Authorize(Roles = "Admin,Staff")]
public class PartsController(IPartService partService) : ControllerBase
{
    /// <summary>
    /// Returns all parts with optional filtering and pagination.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] PartQueryDto query)
    {
        var result = await partService.GetAllAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Returns a single part by ID with full details.
    /// </summary>
    [HttpGet("{partId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int partId)
    {
        var result = await partService.GetByIdAsync(partId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Returns featured parts.
    /// </summary>
    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeatured()
    {
        var result = await partService.GetFeaturedAsync();
        return Ok(result);
    }

    /// <summary>
    /// Returns related parts for a given part.
    /// </summary>
    [HttpGet("{partId:int}/related")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRelated(int partId)
    {
        var result = await partService.GetRelatedAsync(partId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Creates a new part. Admin and Staff can create parts.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartDto dto)
    {
        var result = await partService.CreateAsync(dto);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Updates an existing part. Admin and Staff can update parts.
    /// </summary>
    [HttpPut("{partId:int}")]
    public async Task<IActionResult> Update(int partId, [FromBody] UpdatePartDto dto)
    {
        var result = await partService.UpdateAsync(partId, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Deactivates or activates a part. Admin and Staff can do this.
    /// </summary>
    [HttpPatch("{partId:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int partId, [FromBody] PartDeactivateDto dto)
    {
        var result = await partService.DeactivateAsync(partId, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Soft-deletes a part. Only Admin can perform this one-way deactivation.
    /// </summary>
    [HttpDelete("{partId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int partId)
    {
        var result = await partService.DeleteAsync(partId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
