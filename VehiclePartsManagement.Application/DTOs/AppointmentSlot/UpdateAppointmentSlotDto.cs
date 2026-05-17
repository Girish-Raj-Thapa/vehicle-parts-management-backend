using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.AppointmentSlot;

/// <summary>
/// Request payload for updating an existing appointment slot.
/// </summary>
public class UpdateAppointmentSlotDto
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
