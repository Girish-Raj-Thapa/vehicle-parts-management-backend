using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.PartRequest;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IPartRequestService
{
    Task<ApiResponse<PartRequestCreateResponseDto>> CreateAsync(
        CreatePartRequestDto dto,
        string applicationUserId);

    Task<PagedResponse<List<PartRequestListItemDto>>> GetPagedAsync(
        PartRequestQueryDto query,
        string? applicationUserId);

    Task<ApiResponse<PartRequestDetailDto>> GetByIdAsync(
        int partRequestId,
        string? applicationUserId);

    Task<ApiResponse<PartRequestUpdateResponseDto>> UpdateAsync(
        int partRequestId,
        UpdatePartRequestDto dto);
}
