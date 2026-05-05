using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.PartCategory;

public class UpdatePartCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string CategoryDescription { get; set; } = string.Empty;
}
