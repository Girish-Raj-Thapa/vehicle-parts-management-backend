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
    /// Returns all parts with their category name and stock info.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await partService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Returns a single part by ID with full details.
    /// </summary>
    [HttpGet("{partId:int}")]
    public async Task<IActionResult> GetById(int partId)
    {
        var result = await partService.GetByIdAsync(partId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Creates a new part. Only Admin can create parts.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePartDto dto)
    {
        var result = await partService.CreateAsync(dto);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Updates an existing part. Only Admin can update parts.
    /// </summary>
    [HttpPut("{partId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int partId, [FromBody] UpdatePartDto dto)
    {
        var result = await partService.UpdateAsync(partId, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Deletes a part. Only Admin can delete parts.
    /// </summary>
    [HttpDelete("{partId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int partId)
    {
        var result = await partService.DeleteAsync(partId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
