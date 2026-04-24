using Microsoft.AspNetCore.Identity;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs;
using VehiclePartsManagement.Application.Interfaces;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<ApiResponse> RegisterAsync(RegisterDto dto, string role)
    {
        var requestedRole = role.Trim();
        var roleEntity = await roleManager.FindByNameAsync(requestedRole);
        if (roleEntity?.Name is null)
        {
            return ApiResponse.FailureResponse(
                message: "Invalid role selected.",
                errors: new Dictionary<string, List<string>>
                {
                    { "role", ["Role does not exist."] }
                });
        }

        var existingUser = await userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return ApiResponse.FailureResponse("User with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address ?? string.Empty,
            RegisteredAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return ApiResponse.FailureResponse(
                message: "User registration failed.",
                errors: new Dictionary<string, List<string>>
                {
                    { "identity", result.Errors.Select(e => e.Description).ToList() }
                });
        }

        var roleAssignmentResult = await userManager.AddToRoleAsync(user, roleEntity.Name);
        if (!roleAssignmentResult.Succeeded)
        {
            // Roll back user creation to avoid leaving a user without intended role assignment.
            await userManager.DeleteAsync(user);

            return ApiResponse.FailureResponse(
                message: "User registration failed.",
                errors: new Dictionary<string, List<string>>
                {
                    { "identity", roleAssignmentResult.Errors.Select(e => e.Description).ToList() }
                });
        }

        return ApiResponse.SuccessResponse("User created successfully.");
    }


    public async Task<ApiResponse<LoginResultDto>> LoginAsync(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
        {
            return ApiResponse<LoginResultDto>.FailureResponse("Invalid email or password.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiresAtUtc) = await jwtTokenService.GenerateAccessTokenAsync(user);
        var (refreshToken, refreshTokenExpiresAtUtc) = await jwtTokenService.GenerateRefreshTokenAsync(user);

        return ApiResponse<LoginResultDto>.SuccessResponse(new LoginResultDto
        {
            Response = new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList(),
                AccessToken = accessToken,
                ExpiresAtUtc = expiresAtUtc
            },
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc
        });
    }


    public async Task<ApiResponse<RefreshResultDto>> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return ApiResponse<RefreshResultDto>.FailureResponse("Unauthorized.");
        }

        var user = await jwtTokenService.GetUserFromRefreshTokenAsync(refreshToken);
        if (user is null || !user.IsActive)
        {
            return ApiResponse<RefreshResultDto>.FailureResponse("Unauthorized.");
        }

        var (accessToken, expiresAtUtc) = await jwtTokenService.GenerateAccessTokenAsync(user);
        var (newRefreshToken, refreshTokenExpiresAtUtc) = await jwtTokenService.GenerateRefreshTokenAsync(user);

        return ApiResponse<RefreshResultDto>.SuccessResponse(new RefreshResultDto
        {
            Response = new RefreshAccessTokenResponseDto
            {
                AccessToken = accessToken,
                ExpiresAtUtc = expiresAtUtc
            },
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc
        }, "Access token refreshed successfully.");
    }


    public Task<ApiResponse> LogoutAsync()
    {
        return Task.FromResult(ApiResponse.SuccessResponse("Logged out successfully."));
    }


    public async Task<ApiResponse<UserProfileDto>> MeAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ApiResponse<UserProfileDto>.FailureResponse("User not found.");
        }

        var roles = await userManager.GetRolesAsync(user);

        return ApiResponse<UserProfileDto>.SuccessResponse(new UserProfileDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList()
        });
    }
}
