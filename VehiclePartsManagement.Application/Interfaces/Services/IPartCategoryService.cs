using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.PartCategory;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IPartCategoryService
{
    Task<ApiResponse<List<PartCategoryResponseDto>>> GetAllAsync();
    Task<ApiResponse<PartCategoryResponseDto>> CreateAsync(CreatePartCategoryDto dto);
    Task<ApiResponse<PartCategoryResponseDto>> UpdateAsync(int categoryId, UpdatePartCategoryDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int categoryId);
}
