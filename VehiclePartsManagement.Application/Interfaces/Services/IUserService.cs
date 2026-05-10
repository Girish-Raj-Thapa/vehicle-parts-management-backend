using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Customer;
using VehiclePartsManagement.Application.DTOs.User;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IUserService
{
    Task<PagedResponse<List<UserListItemDto>>> GetPagedAsync(int page, int pageSize, string? role, string? search);
    /// <summary>
    /// Searches customer profiles for staff/admin workflows using paged output.
    /// </summary>
    Task<PagedResponse<List<CustomerSearchItemDto>>> SearchCustomersAsync(CustomerSearchQueryDto dto);
    Task<PagedResponse<List<CustomerPurchaseHistoryListItemDto>>> GetMyPurchaseHistoryAsync(
        string customerUserId,
        int page,
        int pageSize,
        string? paymentStatus,
        DateTime? fromDate,
        DateTime? toDate);
    Task<ApiResponse<CustomerPurchaseDetailDto>> GetMyPurchaseDetailAsync(string customerUserId, int saleId);
    Task<PagedResponse<List<CustomerServiceRecordListItemDto>>> GetMyServiceRecordsAsync(
        string customerUserId,
        int page,
        int pageSize,
        int? vehicleId);
    Task<ApiResponse<CustomerServiceRecordDetailDto>> GetMyServiceRecordDetailAsync(
        string customerUserId,
        int serviceRecordId);
    Task<PagedResponse<List<CustomerPaymentHistoryListItemDto>>> GetMyPaymentHistoryAsync(
        string customerUserId,
        int page,
        int pageSize);
    Task<PagedResponse<List<CustomerAppointmentHistoryListItemDto>>> GetMyAppointmentHistoryAsync(
        string customerUserId,
        int page,
        int pageSize,
        string? status);
    Task<ApiResponse<CustomerHistorySummaryDto>> GetMyHistorySummaryAsync(string customerUserId);
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
    Task<ApiResponse<CustomerDetailsDto>> GetCustomerDetailsAsync(string customerId);
    Task<ApiResponse<CustomerHistoryDto>> GetCustomerHistoryAsync(string customerId);
    Task<ApiResponse<CustomerLoyaltyDto>> GetCustomerLoyaltyAsync(string customerId);
}
