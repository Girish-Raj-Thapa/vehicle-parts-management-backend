using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Vehicle;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IVehicleService
{
    Task<ApiResponse<VehicleRegistrationResponseDto>> RegisterAsync(
        VehicleRegistrationDto dto,
        string userId);

    Task<PagedResponse<List<VehicleListItemDto>>> GetPagedAsync(
        VehicleQueryDto query,
        string? applicationUserId);

    Task<ApiResponse<VehicleDetailDto>> GetByIdAsync(
        int vehicleId,
        string? applicationUserId);

    Task<PagedResponse<List<VehicleServiceHistoryDto>>> GetServiceHistoryAsync(
        int vehicleId,
        string? applicationUserId,
        int page,
        int pageSize);

    Task<PagedResponse<List<VehicleSaleHistoryDto>>> GetSaleHistoryAsync(
        int vehicleId,
        string? applicationUserId,
        int page,
        int pageSize);
}
