using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.AppointmentSlot;

/// <summary>
/// Query contract for appointment slot listing endpoint.
/// </summary>
public class AppointmentSlotListQueryDto
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 200)]
    public int PageSize { get; set; } = 10;

    public DateOnly? Date { get; set; }

    public bool? IsActive { get; set; }
}
