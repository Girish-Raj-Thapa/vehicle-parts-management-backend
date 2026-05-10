using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Customer;
using VehiclePartsManagement.Application.DTOs.User;
using VehiclePartsManagement.Application.Interfaces;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IEmailService emailService,
    IVehicleRepository vehicleRepository,
    IRepositoryBase<VehicleModel> vehicleModelRepository,
    IRepositoryBase<VehicleBrand> vehicleBrandRepository) : IUserService
{
    private static readonly string[] StaffCreatableRoles = ["Admin", "Staff"];


    public async Task<PagedResponse<List<UserListItemDto>>> GetPagedAsync(
        int page,
        int pageSize,
        string? role,
        string? search)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        var users = await userManager.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var userDtos = new List<UserListItemDto>();
        foreach (var user in users)
        {
            var roles = (await userManager.GetRolesAsync(user)).ToList();
            userDtos.Add(new UserListItemDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsActive = user.IsActive,
                Roles = roles
            });
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            userDtos = userDtos
                .Where(u => u.Roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            userDtos = userDtos
                .Where(u =>
                    u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var totalItems = userDtos.Count;
        var data = userDtos
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedResponse<List<UserListItemDto>>.SuccessPagedResponse(
            data,
            new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            },
            "Users retrieved successfully.");
    }


    public async Task<ApiResponse<UserDetailDto>> GetByIdAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ApiResponse<UserDetailDto>.FailureResponse("User not found.");
        }

        return ApiResponse<UserDetailDto>.SuccessResponse(await MapUserDetailAsync(user), "User retrieved successfully.");
    }


    public async Task<ApiResponse<UserDetailDto>> CreateStaffAsync(CreateStaffDto dto)
    {
        var roles = dto.Roles.Count == 0
            ? new List<string> { "Staff" }
            : dto.Roles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var invalidRoleResponse = await ValidateRolesAsync(roles, StaffCreatableRoles);
        if (invalidRoleResponse is not null)
        {
            return ApiResponse<UserDetailDto>.FailureResponse(invalidRoleResponse);
        }

        if (await userManager.FindByEmailAsync(dto.Email) is not null)
        {
            return ApiResponse<UserDetailDto>.FailureResponse("User with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            RegisteredAt = DateTime.UtcNow,
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return IdentityFailure<UserDetailDto>("Staff registration failed.", createResult);
        }

        var roleResult = await userManager.AddToRolesAsync(user, roles);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return IdentityFailure<UserDetailDto>("Staff registration failed.", roleResult);
        }

        var message = "Staff registered successfully.";
        try
        {
            await emailService.SendStaffRegistrationEmailAsync(
                dto.Email,
                dto.FullName,
                dto.Email,
                dto.Password);
        }
        catch
        {
            message = "Staff registered successfully, but email notification could not be sent.";
        }

        return ApiResponse<UserDetailDto>.SuccessResponse(await MapUserDetailAsync(user), message);
    }


    public async Task<ApiResponse<UserIdResponseDto>> UpdateAsync(string userId, UpdateUserDto dto)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ApiResponse<UserIdResponseDto>.FailureResponse("User not found.");
        }

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.Address = dto.Address;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? ApiResponse<UserIdResponseDto>.SuccessResponse(new UserIdResponseDto { UserId = user.Id }, "User updated successfully.")
            : IdentityFailure<UserIdResponseDto>("User update failed.", result);
    }


    public async Task<ApiResponse<UserStatusResponseDto>> UpdateStatusAsync(string userId, UpdateUserStatusDto dto)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ApiResponse<UserStatusResponseDto>.FailureResponse("User not found.");
        }

        user.IsActive = dto.IsActive;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? ApiResponse<UserStatusResponseDto>.SuccessResponse(
                new UserStatusResponseDto { UserId = user.Id, IsActive = user.IsActive },
                "User status updated successfully.")
            : IdentityFailure<UserStatusResponseDto>("User status update failed.", result);
    }


    public async Task<ApiResponse<UserRolesResponseDto>> UpdateRolesAsync(string userId, UpdateUserRolesDto dto)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ApiResponse<UserRolesResponseDto>.FailureResponse("User not found.");
        }

        var roles = dto.Roles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var invalidRoleResponse = await ValidateRolesAsync(roles);
        if (invalidRoleResponse is not null)
        {
            return ApiResponse<UserRolesResponseDto>.FailureResponse(invalidRoleResponse);
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            return IdentityFailure<UserRolesResponseDto>("User roles update failed.", removeResult);
        }

        var addResult = await userManager.AddToRolesAsync(user, roles);
        if (!addResult.Succeeded)
        {
            return IdentityFailure<UserRolesResponseDto>("User roles update failed.", addResult);
        }

        return ApiResponse<UserRolesResponseDto>.SuccessResponse(
            new UserRolesResponseDto { UserId = user.Id, Roles = roles },
            "User roles updated successfully.");
    }


    public async Task<ApiResponse<List<RoleResponseDto>>> GetRolesAsync()
    {
        var roles = await roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => new RoleResponseDto
            {
                RoleId = r.Id,
                RoleName = r.Name ?? string.Empty
            })
            .ToListAsync();

        return ApiResponse<List<RoleResponseDto>>.SuccessResponse(roles, "Roles retrieved successfully.");
    }


    public Task<ApiResponse<UserDetailDto>> GetProfileAsync(string userId) => GetByIdAsync(userId);


    public Task<ApiResponse<UserIdResponseDto>> UpdateProfileAsync(string userId, UpdateUserDto dto) =>
        UpdateAsync(userId, dto);


    public async Task<ApiResponse<CustomerVehicleRegistrationResponseDto>> CreateCustomerWithVehicleAsync(
        CreateCustomerWithVehicleDto dto,
        string createdByUserId)
    {
        if (await userManager.FindByEmailAsync(dto.Email) is not null)
        {
            return ApiResponse<CustomerVehicleRegistrationResponseDto>.FailureResponse("User with this email already exists.");
        }

        var vehicleValidation = await ValidateVehicleAsync(dto.Vehicle.VehicleNumber);
        if (vehicleValidation is not null)
        {
            return ApiResponse<CustomerVehicleRegistrationResponseDto>.FailureResponse(vehicleValidation);
        }

        var generatedPassword = string.IsNullOrWhiteSpace(dto.Password) ? GenerateTemporaryPassword() : null;
        var password = dto.Password ?? generatedPassword!;
        var customer = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            RegisteredAt = DateTime.UtcNow,
            IsActive = true,
            CreatedByUserId = createdByUserId
        };

        var createResult = await userManager.CreateAsync(customer, password);
        if (!createResult.Succeeded)
        {
            return IdentityFailure<CustomerVehicleRegistrationResponseDto>("Customer registration failed.", createResult);
        }

        var roleResult = await userManager.AddToRoleAsync(customer, "Customer");
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(customer);
            return IdentityFailure<CustomerVehicleRegistrationResponseDto>("Customer registration failed.", roleResult);
        }

        var (modelId, errorMessage) = await ResolveVehicleModelIdAsync(dto.Vehicle);
        if (errorMessage is not null)
        {
            await userManager.DeleteAsync(customer);
            return ApiResponse<CustomerVehicleRegistrationResponseDto>.FailureResponse(errorMessage);
        }

        var vehicle = new Vehicle
        {
            ApplicationUserId = customer.Id,
            VehicleModelId = modelId,
            VehicleNumber = dto.Vehicle.VehicleNumber,
            ManufactureYear = dto.Vehicle.ManufactureYear,
            EngineNumber = dto.Vehicle.EngineNumber,
            ChassisNumber = dto.Vehicle.ChassisNumber,
            FuelType = dto.Vehicle.FuelType,
            Color = dto.Vehicle.Color,
            CurrentMileage = dto.Vehicle.CurrentMileage,
            VehicleRegisteredAt = DateTime.UtcNow
        };

        vehicleRepository.Create(vehicle);
        await vehicleRepository.SaveChangesAsync();

        return ApiResponse<CustomerVehicleRegistrationResponseDto>.SuccessResponse(
            new CustomerVehicleRegistrationResponseDto
            {
                UserId = customer.Id,
                VehicleId = vehicle.VehicleId,
                TemporaryPassword = generatedPassword
            },
            "Customer registered successfully.");
    }


    private async Task<UserDetailDto> MapUserDetailAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new UserDetailDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Address = user.Address,
            IsActive = user.IsActive,
            Roles = roles.ToList()
        };
    }


    private async Task<string?> ValidateRolesAsync(IEnumerable<string> roles, IReadOnlyCollection<string>? allowedRoles = null)
    {
        foreach (var role in roles)
        {
            if (allowedRoles is not null && !allowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                return $"Role '{role}' cannot be assigned from this endpoint.";
            }

            if (!await roleManager.RoleExistsAsync(role))
            {
                return $"Role '{role}' does not exist.";
            }
        }

        return null;
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


    private static ApiResponse<T> IdentityFailure<T>(string message, IdentityResult result) =>
        ApiResponse<T>.FailureResponse(
            message,
            new Dictionary<string, List<string>>
            {
                { "identity", result.Errors.Select(e => e.Description).ToList() }
            });


    private static string GenerateTemporaryPassword()
    {
        var token = Guid.NewGuid().ToString("N")[..8];
        return $"Temp@{token}a1";
    }
}
