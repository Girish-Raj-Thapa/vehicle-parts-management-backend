using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Domain.Entities;

public class VehicleBrand
{
    [Key]
    public int VehicleBrandId { get; set; }

    [Required]
    [MaxLength(100)]
    public string BrandName { get; set; } = string.Empty;

    public ICollection<VehicleModel> VehicleModels { get; set; } = new List<VehicleModel>();
}
