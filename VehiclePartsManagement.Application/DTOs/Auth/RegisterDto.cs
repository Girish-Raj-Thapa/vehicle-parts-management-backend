using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs;

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

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    [Required]
    public string Role { get; set; } = string.Empty;
}
