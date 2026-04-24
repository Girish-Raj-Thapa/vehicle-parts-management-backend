namespace VehiclePartsManagement.Application.DTOs.Auth;

public class RefreshAccessTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
