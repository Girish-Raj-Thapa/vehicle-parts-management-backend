using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces;

/// <summary>
/// Provides token generation operations for JWT authentication.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates an access token for the given user.
    /// </summary>
    Task<(string AccessToken, DateTime ExpiresAtUtc)> GenerateAccessTokenAsync(ApplicationUser user);


    /// <summary>
    /// Generates a signed refresh token for the given user.
    /// </summary>
    Task<(string RefreshToken, DateTime ExpiresAtUtc)> GenerateRefreshTokenAsync(ApplicationUser user);


    /// <summary>
    /// Validates a refresh token and resolves its associated user.
    /// </summary>
    Task<ApplicationUser?> GetUserFromRefreshTokenAsync(string refreshToken);
}
