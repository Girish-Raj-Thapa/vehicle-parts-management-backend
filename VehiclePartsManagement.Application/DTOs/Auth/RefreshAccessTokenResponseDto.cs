namespace VehiclePartsManagement.Application.DTOs;

public class RefreshAccessTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
