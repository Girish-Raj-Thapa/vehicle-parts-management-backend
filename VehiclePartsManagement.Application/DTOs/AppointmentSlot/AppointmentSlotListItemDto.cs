namespace VehiclePartsManagement.Application.DTOs.AppointmentSlot;

/// <summary>
/// List row payload for appointment slot responses.
/// </summary>
public class AppointmentSlotListItemDto
{
    public int AppointmentSlotId { get; set; }
    public DateOnly SlotDate { get; set; }
    public string SlotTime { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public bool IsActive { get; set; }
}
