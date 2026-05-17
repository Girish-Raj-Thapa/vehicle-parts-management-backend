using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Domain.Entities;

/// <summary>
/// Defines a bookable service appointment slot and its capacity.
/// </summary>
public class AppointmentSlot
{
    [Key]
    public int AppointmentSlotId { get; set; }

    public DateTime SlotDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string SlotTime { get; set; } = string.Empty;

    public int MaxCapacity { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
