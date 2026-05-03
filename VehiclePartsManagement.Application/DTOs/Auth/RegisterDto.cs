using System.ComponentModel.DataAnnotations;
using VehiclePartsManagement.Application.DTOs.Vehicle;

namespace VehiclePartsManagement.Application.DTOs.Auth;

public class RegisterDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Compare(nameof(Password))]
    public string? ConfirmPassword { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string Role { get; set; } = "Customer";

    public VehicleRegistrationDto? Vehicle { get; set; }
}
