using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Customer;
using VehiclePartsManagement.Application.DTOs.User;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IUserService
{
    Task<PagedResponse<List<UserListItemDto>>> GetPagedAsync(int page, int pageSize, string? role, string? search);
    Task<ApiResponse<UserDetailDto>> GetByIdAsync(string userId);
    Task<ApiResponse<UserDetailDto>> CreateStaffAsync(CreateStaffDto dto);
    Task<ApiResponse<UserIdResponseDto>> UpdateAsync(string userId, UpdateUserDto dto);
    Task<ApiResponse<UserStatusResponseDto>> UpdateStatusAsync(string userId, UpdateUserStatusDto dto);
    Task<ApiResponse<UserRolesResponseDto>> UpdateRolesAsync(string userId, UpdateUserRolesDto dto);
    Task<ApiResponse<List<RoleResponseDto>>> GetRolesAsync();
    Task<ApiResponse<UserDetailDto>> GetProfileAsync(string userId);
    Task<ApiResponse<UserIdResponseDto>> UpdateProfileAsync(string userId, UpdateUserDto dto);
    Task<ApiResponse<CustomerVehicleRegistrationResponseDto>> CreateCustomerWithVehicleAsync(
        CreateCustomerWithVehicleDto dto,
        string createdByUserId);
}
