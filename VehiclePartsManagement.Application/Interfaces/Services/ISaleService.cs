using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Sale;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface ISaleService
{
    Task<ApiResponse<SaleResponseDto>> CreateAsync(CreateSaleDto dto, string processedByUserId);
}
