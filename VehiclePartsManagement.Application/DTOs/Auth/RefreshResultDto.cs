using VehiclePartsManagement.Application.DTOs.Auth;

namespace VehiclePartsManagement.Application.DTOs;

public class RefreshResultDto
{
    public RefreshAccessTokenResponseDto Response { get; set; } = new();
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAtUtc { get; set; }
}
