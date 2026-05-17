namespace VehiclePartsManagement.Application.DTOs.AppointmentSlot;

/// <summary>
/// Response payload for successful appointment slot update operations.
/// </summary>
public class AppointmentSlotUpdateResponseDto
{
    public int AppointmentSlotId { get; set; }
    public bool IsActive { get; set; }
}
