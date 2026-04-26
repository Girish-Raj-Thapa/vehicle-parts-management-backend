using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.User;

public class UpdateUserRolesDto
{
    [Required]
    [MinLength(1)]
    public List<string> Roles { get; set; } = [];
}
