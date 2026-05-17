using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.AppointmentSlot;

/// <summary>
/// Request payload for creating a new appointment slot.
/// </summary>
public class CreateAppointmentSlotDto
{
    [Required]
    public DateOnly SlotDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string SlotTime { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int MaxCapacity { get; set; } = 1;

    public bool IsActive { get; set; } = true;
}
