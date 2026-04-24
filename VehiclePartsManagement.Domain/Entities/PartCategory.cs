using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Domain.Entities;

public class PartCategory
{
    [Key]
    public int PartCategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string CategoryDescription { get; set; } = string.Empty;

    public ICollection<Part> Parts { get; set; } = new List<Part>();
}
