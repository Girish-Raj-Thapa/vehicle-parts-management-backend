using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Purchase;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IPurchaseService
{
    Task<PagedResponse<List<PurchaseListItemDto>>> GetPagedAsync(PurchaseQueryDto query);

    Task<ApiResponse<PurchaseDetailDto>> GetByIdAsync(int purchaseId);

    Task<ApiResponse<PurchaseCreateResponseDto>> CreateAsync(
        CreatePurchaseDto dto,
        string createdByApplicationUserId);
}
