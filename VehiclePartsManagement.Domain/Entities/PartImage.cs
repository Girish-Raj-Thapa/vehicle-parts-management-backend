using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiclePartsManagement.Domain.Entities;

public class PartImage
{
    [Key]
    public int PartImageId { get; set; }

    [Required]
    public int PartId { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImagePath { get; set; } = string.Empty;

    public bool IsPrimary { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(PartId))]
    public Part Part { get; set; } = null!;
}
