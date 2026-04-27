using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerSearchQueryDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(150)]
    public string Query { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(name|phone|id|vehicle)$", ErrorMessage = "Type must be one of: name, phone, id, vehicle.")]
    public string Type { get; set; } = "name";

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
