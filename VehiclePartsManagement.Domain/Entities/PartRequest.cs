using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Domain.Entities;

public class PartRequest
{
    [Key]
    public int PartRequestId { get; set; }

    [Required]
    [MaxLength(450)]
    public string ApplicationUserId { get; set; } = string.Empty;

    public int? VehicleId { get; set; }

    public int? FulfilledPartId { get; set; }

    [Required]
    [MaxLength(150)]
    public string RequestedPartName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string RequestDescription { get; set; } = string.Empty;

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    [Required]
    public PartRequestStatus RequestStatus { get; set; } = PartRequestStatus.Pending;

    [ForeignKey(nameof(ApplicationUserId))]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(VehicleId))]
    public Vehicle? Vehicle { get; set; }

    [ForeignKey(nameof(FulfilledPartId))]
    public Part? FulfilledPart { get; set; }
}
