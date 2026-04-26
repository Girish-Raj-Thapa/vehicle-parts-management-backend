using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.User;

public class CreateStaffDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Phone]
    [MaxLength(10)]
    public string? PhoneNumber { get; set; }

    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = ["Staff"];
}
