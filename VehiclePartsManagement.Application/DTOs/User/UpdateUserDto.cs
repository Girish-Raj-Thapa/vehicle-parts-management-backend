using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.User;

public class UpdateUserDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    [MaxLength(10)]
    public string? PhoneNumber { get; set; }

    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;
}
