using Microsoft.AspNetCore.Identity;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs;
using VehiclePartsManagement.Application.DTOs.Auth;
using VehiclePartsManagement.Application.Interfaces;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IJwtTokenService jwtTokenService,
    IVehicleRepository vehicleRepository,
    IRepositoryBase<VehicleModel> vehicleModelRepository,
    IRepositoryBase<VehicleBrand> vehicleBrandRepository) : IAuthService
{
    public async Task<ApiResponse<UserProfileDto>> RegisterAsync(RegisterDto dto, string role)
    {
        var requestedRole = string.IsNullOrWhiteSpace(role) ? "Customer" : role.Trim();
        if (!string.Equals(requestedRole, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<UserProfileDto>.FailureResponse("Customer self-registration can only create Customer accounts.");
        }

        var roleEntity = await roleManager.FindByNameAsync(requestedRole);
        if (roleEntity?.Name is null)
        {
            return ApiResponse<UserProfileDto>.FailureResponse(
                message: "Invalid role selected.",
                errors: new Dictionary<string, List<string>>
                {
                    { "role", ["Role does not exist."] }
                });
        }

        var existingUser = await userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return ApiResponse<UserProfileDto>.FailureResponse("User with this email already exists.");
        }

        if (dto.Vehicle is not null)
        {
            var vehicleValidation = await ValidateVehicleAsync(dto.Vehicle.VehicleNumber);
            if (vehicleValidation is not null)
            {
                return ApiResponse<UserProfileDto>.FailureResponse(vehicleValidation);
            }
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
            return ApiResponse<UserProfileDto>.FailureResponse(
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

            return ApiResponse<UserProfileDto>.FailureResponse(
                message: "User registration failed.",
                errors: new Dictionary<string, List<string>>
                {
                    { "identity", roleAssignmentResult.Errors.Select(e => e.Description).ToList() }
                });
        }

        if (dto.Vehicle is not null)
        {
            var (modelId, errorMessage) = await ResolveVehicleModelIdAsync(dto.Vehicle);
            if (errorMessage is not null)
            {
                await userManager.DeleteAsync(user);
                return ApiResponse<UserProfileDto>.FailureResponse(errorMessage);
            }

            vehicleRepository.Create(new Vehicle
            {
                ApplicationUserId = user.Id,
                VehicleModelId = modelId,
                VehicleNumber = dto.Vehicle.VehicleNumber,
                ManufactureYear = dto.Vehicle.ManufactureYear,
                EngineNumber = dto.Vehicle.EngineNumber,
                ChassisNumber = dto.Vehicle.ChassisNumber,
                FuelType = dto.Vehicle.FuelType,
                Color = dto.Vehicle.Color,
                CurrentMileage = dto.Vehicle.CurrentMileage,
                VehicleRegisteredAt = DateTime.UtcNow
            });
            await vehicleRepository.SaveChangesAsync();
        }

        return ApiResponse<UserProfileDto>.SuccessResponse(new UserProfileDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Address = user.Address,
            IsActive = user.IsActive,
            Roles = [roleEntity.Name]
        }, "Customer registered successfully.");
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
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Address = user.Address,
            IsActive = user.IsActive,
            Roles = roles.ToList()
        }, "Current user retrieved successfully.");
    }


    private Task<string?> ValidateVehicleAsync(string vehicleNumber)
    {
        var vehicleExists = vehicleRepository
            .FindByCondition(v => v.VehicleNumber == vehicleNumber)
            .Any();

        return Task.FromResult(vehicleExists ? "Vehicle number already exists." : null);
    }


    private async Task<(int ModelId, string? ErrorMessage)> ResolveVehicleModelIdAsync(
        VehiclePartsManagement.Application.DTOs.Vehicle.VehicleRegistrationDto dto)
    {
        if (dto.ModelId is > 0)
        {
            var existingModel = await vehicleModelRepository.GetByIdAsync(dto.ModelId.Value);
            return existingModel is null
                ? (0, "Vehicle model not found.")
                : (existingModel.VehicleModelId, null);
        }

        if (string.IsNullOrWhiteSpace(dto.BrandName) || string.IsNullOrWhiteSpace(dto.ModelName))
        {
            return (0, "Either modelId or both brandName and modelName are required.");
        }

        var brandName = dto.BrandName.Trim();
        var modelName = dto.ModelName.Trim();

        var brand = vehicleBrandRepository
            .FindByCondition(b => b.BrandName == brandName, trackChanges: true)
            .FirstOrDefault();

        if (brand is null)
        {
            brand = new VehicleBrand { BrandName = brandName };
            vehicleBrandRepository.Create(brand);
            await vehicleBrandRepository.SaveChangesAsync();
        }

        var model = vehicleModelRepository
            .FindByCondition(m => m.VehicleBrandId == brand.VehicleBrandId && m.ModelName == modelName, trackChanges: true)
            .FirstOrDefault();

        if (model is null)
        {
            model = new VehicleModel
            {
                VehicleBrandId = brand.VehicleBrandId,
                ModelName = modelName
            };
            vehicleModelRepository.Create(model);
            await vehicleModelRepository.SaveChangesAsync();
        }

        return (model.VehicleModelId, null);
    }
}
