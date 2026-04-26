namespace VehiclePartsManagement.Application.DTOs.User;

public class UserRolesResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}
