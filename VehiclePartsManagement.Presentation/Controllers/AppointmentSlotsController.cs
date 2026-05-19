using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.DTOs.AppointmentSlot;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

/// <summary>
/// Staff/admin endpoints for appointment slot management.
/// </summary>
[ApiController]
[Route("api/appointment-slots")]
[Authorize]
public class AppointmentSlotsController(IAppointmentSlotService appointmentSlotService) : ControllerBase
{
    /// <summary>
    /// Returns paginated appointment slots with optional date and active filters.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetSlots([FromQuery] AppointmentSlotListQueryDto query)
    {
        var result = await appointmentSlotService.GetSlotsAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    /// <summary>
    /// Returns one appointment slot by id.
    /// </summary>
    [HttpGet("{appointmentSlotId:int}")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetById(int appointmentSlotId)
    {
        var result = await appointmentSlotService.GetByIdAsync(appointmentSlotId);
        return result.Success ? Ok(result) : NotFound(result);
    }


    /// <summary>
    /// Creates a new appointment slot definition.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentSlotDto dto)
    {
        var result = await appointmentSlotService.CreateAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        var created = (AppointmentSlotDetailDto?)result.Data;
        return CreatedAtAction(nameof(GetById), new { appointmentSlotId = created?.AppointmentSlotId }, result);
    }


    /// <summary>
    /// Updates an existing appointment slot definition.
    /// </summary>
    [HttpPut("{appointmentSlotId:int}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int appointmentSlotId, [FromBody] UpdateAppointmentSlotDto dto)
    {
        var result = await appointmentSlotService.UpdateAsync(appointmentSlotId, dto);
        if (!result.Success)
        {
            return result.Errors.ContainsKey("appointmentSlotId") ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }
}
