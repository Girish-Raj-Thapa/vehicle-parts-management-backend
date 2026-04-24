using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs;
using VehiclePartsManagement.Application.Interfaces;

namespace VehiclePartsManagement.Presentation.Controllers;

/// <summary>
/// Authentication endpoints for register/login/logout and current-user profile.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";


    /// <summary>
    /// Registers a new user for the requested role.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto, dto.Role);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }


    /// <summary>
    /// Authenticates credentials, returns access token in response body,
    /// and sets refresh token as HttpOnly cookie.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        if (!result.Success)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.FailureResponse(result.Message, result.Errors));
        }

        if (result.Data is LoginResultDto data)
        {
            Response.Cookies.Append(
                RefreshTokenCookieName,
                data.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = data.RefreshTokenExpiresAtUtc,
                    Path = "/"
                });

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(data.Response, result.Message));
        }

        return Unauthorized(ApiResponse<AuthResponseDto>.FailureResponse("Invalid email or password."));
    }


    /// <summary>
    /// Issues a new access token using the refresh token from HttpOnly cookie.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken))
        {
            return Unauthorized(ApiResponse<RefreshAccessTokenResponseDto>.FailureResponse("Unauthorized."));
        }

        var result = await authService.RefreshAsync(refreshToken);
        if (!result.Success)
        {
            return Unauthorized(ApiResponse<RefreshAccessTokenResponseDto>.FailureResponse(result.Message, result.Errors));
        }

        if (result.Data is not RefreshResultDto data)
        {
            return Unauthorized(ApiResponse<RefreshAccessTokenResponseDto>.FailureResponse("Unauthorized."));
        }

        Response.Cookies.Append(
            RefreshTokenCookieName,
            data.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = data.RefreshTokenExpiresAtUtc,
                Path = "/"
            });

        return Ok(ApiResponse<RefreshAccessTokenResponseDto>.SuccessResponse(data.Response, result.Message));
    }


    /// <summary>
    /// Performs stateless logout and removes refresh-token cookie.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        Response.Cookies.Delete(
            RefreshTokenCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

        var result = await authService.LogoutAsync();
        return Ok(result);
    }


    /// <summary>
    /// Returns profile details for the authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized."));
        }

        var result = await authService.MeAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
