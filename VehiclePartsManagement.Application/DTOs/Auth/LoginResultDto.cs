using VehiclePartsManagement.Application.DTOs.Auth;

namespace VehiclePartsManagement.Application.DTOs;

public class LoginResultDto
{
    public AuthResponseDto Response { get; set; } = new();
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAtUtc { get; set; }
}
