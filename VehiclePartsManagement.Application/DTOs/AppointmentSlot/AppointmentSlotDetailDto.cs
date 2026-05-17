namespace VehiclePartsManagement.Application.DTOs.AppointmentSlot;

/// <summary>
/// Detailed payload for a single appointment slot.
/// </summary>
public class AppointmentSlotDetailDto
{
    public int AppointmentSlotId { get; set; }
    public DateOnly SlotDate { get; set; }
    public string SlotTime { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public bool IsActive { get; set; }
}
