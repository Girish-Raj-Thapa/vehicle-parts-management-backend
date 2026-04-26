namespace VehiclePartsManagement.Application.DTOs.User;

public class UserStatusResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
