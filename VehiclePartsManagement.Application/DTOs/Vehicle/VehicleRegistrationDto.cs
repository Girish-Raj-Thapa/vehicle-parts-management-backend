using System.ComponentModel.DataAnnotations;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Application.DTOs.Vehicle;

public class VehicleRegistrationDto
{
    [Required]
    public int VehicleModelId { get; set; }

    [Required]
    [MaxLength(50)]
    public string VehicleNumber { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int? ManufactureYear { get; set; }

    [MaxLength(100)]
    public string EngineNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ChassisNumber { get; set; } = string.Empty;

    [Required]
    public FuelType FuelType { get; set; } = FuelType.Petrol;

    [MaxLength(30)]
    public string Color { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int CurrentMileage { get; set; }
}
