using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace VehiclePartsManagement.Infrastructure.Config;

/// <summary>
/// Strongly-typed JWT configuration bound from app settings.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    [MinLength(32)]
    public string Secret { get; init; } = string.Empty;

    /// <summary>
    /// Signing key derived from configured secret.
    /// </summary>
    public SymmetricSecurityKey SymmetricSecurityKey =>
        new(Encoding.UTF8.GetBytes(Secret));

    [Range(1, int.MaxValue)]
    public int ExpiryHours { get; init; }

    [Range(1, int.MaxValue)]
    public int RefreshTokenExpiryDays { get; init; } = 7;

    /// <summary>
    /// Access-token expiry timestamp (UTC).
    /// </summary>
    public DateTime AccessTokenExpiryDate =>
        DateTime.UtcNow.AddHours(ExpiryHours);

    /// <summary>
    /// Refresh-token expiry timestamp (UTC).
    /// </summary>
    public DateTime RefreshTokenExpiryDate =>
        DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);
}
