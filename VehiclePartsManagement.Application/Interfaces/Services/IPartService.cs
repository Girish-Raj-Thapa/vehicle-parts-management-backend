using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Part;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IPartService
{
    Task<ApiResponse<List<PartListItemDto>>> GetAllAsync();
    Task<ApiResponse<PartResponseDto>> GetByIdAsync(int partId);
    Task<ApiResponse<PartResponseDto>> CreateAsync(CreatePartDto dto);
    Task<ApiResponse<PartResponseDto>> UpdateAsync(int partId, UpdatePartDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int partId);
}
