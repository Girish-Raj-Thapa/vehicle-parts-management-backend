using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiclePartsManagement.Domain.Entities;

public class VehicleModel
{
    [Key]
    public int VehicleModelId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ModelName { get; set; } = string.Empty;

    [Required]
    public int VehicleBrandId { get; set; }

    [ForeignKey(nameof(VehicleBrandId))]
    public VehicleBrand Brand { get; set; } = null!;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<PartCompatibility> PartCompatibilities { get; set; } = new List<PartCompatibility>();
}
