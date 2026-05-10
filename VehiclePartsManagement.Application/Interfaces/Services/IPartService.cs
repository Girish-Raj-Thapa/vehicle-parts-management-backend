using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Inventory;
using VehiclePartsManagement.Application.DTOs.Part;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IPartService
{
    Task<ApiResponse<IEnumerable<LowStockPartDto>>> GetLowStockPartsAsync();
    Task<PagedResponse<List<PartListItemDto>>> GetAllAsync(PartQueryDto query);
    Task<ApiResponse<PartResponseDto>> GetByIdAsync(int partId);
    Task<ApiResponse<PartResponseDto>> CreateAsync(CreatePartDto dto);
    Task<ApiResponse<PartResponseDto>> UpdateAsync(int partId, UpdatePartDto dto);
    Task<ApiResponse<PartStatusResponseDto>> DeactivateAsync(int partId, PartDeactivateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int partId);
    Task<ApiResponse<List<PartFeaturedItemDto>>> GetFeaturedAsync();
    Task<ApiResponse<List<PartListItemDto>>> GetRelatedAsync(int partId);
}
