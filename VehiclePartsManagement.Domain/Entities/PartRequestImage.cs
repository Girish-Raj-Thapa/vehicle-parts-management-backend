using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiclePartsManagement.Domain.Entities;

public class PartRequestImage
{
    [Key]
    public int PartRequestImageId { get; set; }

    [Required]
    public int PartRequestId { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImagePath { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(PartRequestId))]
    public PartRequest PartRequest { get; set; } = null!;
}
