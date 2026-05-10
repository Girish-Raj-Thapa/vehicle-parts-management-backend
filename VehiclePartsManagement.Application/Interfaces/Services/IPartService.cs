using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Inventory;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IPartService
{
    Task<ApiResponse<IEnumerable<LowStockPartDto>>> GetLowStockPartsAsync();
}
