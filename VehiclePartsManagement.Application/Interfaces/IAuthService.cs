using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs;
using VehiclePartsManagement.Application.DTOs.Auth;

namespace VehiclePartsManagement.Application.Interfaces;

/// <summary>
/// Defines authentication and current-user operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user with the requested role.
    /// </summary>
    Task<ApiResponse> RegisterAsync(RegisterDto dto, string role);


    /// <summary>
    /// Authenticates a user and returns tokens/payload for login flow.
    /// </summary>
    Task<ApiResponse<LoginResultDto>> LoginAsync(LoginDto dto);


    /// <summary>
    /// Issues a new access token using a valid refresh token.
    /// </summary>
    Task<ApiResponse<RefreshResultDto>> RefreshAsync(string refreshToken);


    /// <summary>
    /// Executes stateless logout behavior.
    /// </summary>
    Task<ApiResponse> LogoutAsync();


    /// <summary>
    /// Returns profile details for the authenticated user.
    /// </summary>
    Task<ApiResponse<UserProfileDto>> MeAsync(string userId);
}
