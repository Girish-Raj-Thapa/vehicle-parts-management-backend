using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiclePartsManagement.Domain.Entities;

public class PartCompatibility
{
    [Required]
    public int PartId { get; set; }

    [Required]
    public int VehicleModelId { get; set; }

    [ForeignKey(nameof(PartId))]
    public Part Part { get; set; } = null!;

    [ForeignKey(nameof(VehicleModelId))]
    public VehicleModel Model { get; set; } = null!;
}
