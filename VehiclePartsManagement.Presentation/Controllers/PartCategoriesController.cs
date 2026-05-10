using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.DTOs.PartCategory;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/part-categories")]
[Authorize(Roles = "Admin,Staff")]
public class PartCategoriesController(IPartCategoryService categoryService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await categoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartCategoryDto dto)
    {
        var result = await categoryService.CreateAsync(dto);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }

    [HttpPut("{categoryId:int}")]
    public async Task<IActionResult> Update(int categoryId, [FromBody] UpdatePartCategoryDto dto)
    {
        var result = await categoryService.UpdateAsync(categoryId, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{categoryId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int categoryId)
    {
        var result = await categoryService.DeleteAsync(categoryId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
