using System.ComponentModel.DataAnnotations;
using VehiclePartsManagement.Application.DTOs.Vehicle;

namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CreateCustomerWithVehicleDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [MaxLength(10)]
    public string? PhoneNumber { get; set; }

    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [MinLength(6)]
    public string? Password { get; set; }

    [Required]
    public VehicleRegistrationDto Vehicle { get; set; } = new();
}
