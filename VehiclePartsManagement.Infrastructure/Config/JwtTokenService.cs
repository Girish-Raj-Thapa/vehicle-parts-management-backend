using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VehiclePartsManagement.Application.Interfaces;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Config;

/// <summary>
/// Generates access and refresh tokens used by the authentication flow.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtTokenService(
        IOptions<JwtOptions> jwtOptions,
        UserManager<ApplicationUser> userManager)
    {
        _jwtOptions = jwtOptions.Value;
        _userManager = userManager;
    }


    /// <inheritdoc />
    public async Task<(string AccessToken, DateTime ExpiresAtUtc)> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var expiresAtUtc = _jwtOptions.AccessTokenExpiryDate;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
        };

        var roles = await _userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var accessToken = GenerateToken(claims, expiresAtUtc);
        return (accessToken, expiresAtUtc);
    }


    /// <inheritdoc />
    public Task<(string RefreshToken, DateTime ExpiresAtUtc)> GenerateRefreshTokenAsync(ApplicationUser user)
    {
        var expiresAtUtc = _jwtOptions.RefreshTokenExpiryDate;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("token_type", "refresh")
        };

        var refreshToken = GenerateToken(claims, expiresAtUtc);
        return Task.FromResult((refreshToken, expiresAtUtc));
    }


    /// <inheritdoc />
    public async Task<ApplicationUser?> GetUserFromRefreshTokenAsync(string refreshToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _jwtOptions.SymmetricSecurityKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
            var tokenType = principal.FindFirstValue("token_type");
            if (!string.Equals(tokenType, "refresh", StringComparison.Ordinal))
            {
                return null;
            }

            var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                         ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return await _userManager.FindByIdAsync(userId);
        }
        catch
        {
            return null;
        }
    }


    /// <summary>
    /// Creates and signs a JWT with configured issuer/audience/key.
    /// </summary>
    private string GenerateToken(IEnumerable<Claim> claims, DateTime expiresAtUtc)
    {
        var credentials = new SigningCredentials(
            _jwtOptions.SymmetricSecurityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
