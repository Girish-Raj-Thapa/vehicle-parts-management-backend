using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.DTOs.Vendor;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

/// <summary>
/// Vendor-management endpoints for administrator users.
/// </summary>
[ApiController]
[Route("api/vendors")]
[Authorize(Roles = "Admin")]
public class VendorsController(IVendorService vendorService) : ControllerBase
{
    /// <summary>
    /// Returns paginated vendor records with optional search and active filter.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetVendors([FromQuery] VendorListQueryDto query)
    {
        var result = await vendorService.GetVendorsAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    
    /// <summary>
    /// Returns details of a specific vendor.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetVendorById([FromRoute] int id)
    {
        var result = await vendorService.GetVendorByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    
    /// <summary>
    /// Creates a new vendor.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateVendor([FromBody] CreateVendorDto dto)
    {
        var result = await vendorService.CreateVendorAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        var created = (VendorDetailsDto?)result.Data;
        return CreatedAtAction(nameof(GetVendorById), new { id = created?.VendorId }, result);
    }
    
    
    /// <summary>
    /// Updates an existing vendor.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateVendor([FromRoute] int id, [FromBody] UpdateVendorDto dto)
    {
        var result = await vendorService.UpdateVendorAsync(id, dto);
        if (!result.Success)
        {
            return result.Errors.ContainsKey("vendorId") ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    
    /// <summary>
    /// Activates or deactivates a vendor.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateVendorStatus([FromRoute] int id, [FromBody] UpdateVendorStatusDto dto)
    {
        var result = await vendorService.UpdateVendorStatusAsync(id, dto);
        return result.Success
            ? Ok(result)
            : (result.Errors.ContainsKey("vendorId") ? NotFound(result) : BadRequest(result));
    }

    
    /// <summary>
    /// Deletes a vendor when there is no linked purchase history.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteVendor([FromRoute] int id)
    {
        var result = await vendorService.DeleteVendorAsync(id);

        if (!result.Success)
        {
            return result.Errors.ContainsKey("vendorId") && result.Message == "Vendor not found."
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }
    
    
    /// <summary>
    /// Returns paginated purchase history for a vendor.
    /// </summary>
    [HttpGet("{id:int}/purchases")]
    public async Task<IActionResult> GetVendorPurchases(
        [FromRoute] int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var result = await vendorService.GetVendorPurchasesAsync(id, page, pageSize, fromDate, toDate);

        if (!result.Success)
        {
            return result.Errors.ContainsKey("vendorId") ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    
    /// <summary>
    /// Returns aggregate purchase summary for a vendor.
    /// </summary>
    [HttpGet("{id:int}/summary")]
    public async Task<IActionResult> GetVendorSummary([FromRoute] int id)
    {
        var result = await vendorService.GetVendorSummaryAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
