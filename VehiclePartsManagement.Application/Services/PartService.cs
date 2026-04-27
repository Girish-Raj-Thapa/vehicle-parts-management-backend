using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Inventory;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Application.Services;

public class PartService(IPartRepository partRepository) : IPartService
{
    public async Task<ApiResponse<IEnumerable<LowStockPartDto>>> GetLowStockPartsAsync()
    {
        var parts = await partRepository.GetLowStockPartsAsync();
        var data = parts.Select(p => new LowStockPartDto
        {
            PartId = p.PartId,
            Sku = p.Sku,
            PartName = p.PartName,
            CategoryName = p.Category?.CategoryName ?? string.Empty,
            CurrentStock = p.CurrentStock,
            LowStockThreshold = p.LowStockThreshold,
            Status = "Low Stock"
        });

        return ApiResponse<IEnumerable<LowStockPartDto>>.SuccessResponse(data, "Low stock parts retrieved successfully");
    }
}
