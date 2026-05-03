using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Part;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Services;

public class PartService(IPartRepository partRepository) : IPartService
{
    public async Task<ApiResponse<List<PartListItemDto>>> GetAllAsync()
    {
        var parts = await partRepository.GetAllWithCategoryAsync();
        var dtos = parts.Select(MapToListItem).ToList();
        return ApiResponse<List<PartListItemDto>>.SuccessResponse(dtos, "Parts retrieved successfully.");
    }

    public async Task<ApiResponse<PartResponseDto>> GetByIdAsync(int partId)
    {
        var part = await partRepository.GetByIdWithCategoryAsync(partId);
        if (part is null)
            return ApiResponse<PartResponseDto>.FailureResponse("Part not found.");

        return ApiResponse<PartResponseDto>.SuccessResponse(MapToResponse(part), "Part retrieved successfully.");
    }

    public async Task<ApiResponse<PartResponseDto>> CreateAsync(CreatePartDto dto)
    {
        var part = new Part
        {
            PartCategoryId = dto.PartCategoryId,
            Sku = dto.Sku,
            PartName = dto.PartName,
            PartDescription = dto.PartDescription,
            SellingPrice = dto.SellingPrice,
            CurrentStock = 0,
            LowStockThreshold = dto.LowStockThreshold,
            IsActive = true
        };

        partRepository.Create(part);
        await partRepository.SaveChangesAsync();

        // Reload with category for the response
        var created = await partRepository.GetByIdWithCategoryAsync(part.PartId);
        return ApiResponse<PartResponseDto>.SuccessResponse(MapToResponse(created!), "Part created successfully.");
    }

    public async Task<ApiResponse<PartResponseDto>> UpdateAsync(int partId, UpdatePartDto dto)
    {
        var part = await partRepository.GetByIdWithCategoryAsync(partId);
        if (part is null)
            return ApiResponse<PartResponseDto>.FailureResponse("Part not found.");

        if (dto.PartCategoryId.HasValue) part.PartCategoryId = dto.PartCategoryId.Value;
        if (dto.Sku is not null) part.Sku = dto.Sku;
        if (dto.PartName is not null) part.PartName = dto.PartName;
        if (dto.PartDescription is not null) part.PartDescription = dto.PartDescription;
        if (dto.SellingPrice.HasValue) part.SellingPrice = dto.SellingPrice.Value;
        if (dto.LowStockThreshold.HasValue) part.LowStockThreshold = dto.LowStockThreshold.Value;
        if (dto.IsActive.HasValue) part.IsActive = dto.IsActive.Value;

        partRepository.Update(part);
        await partRepository.SaveChangesAsync();

        // Reload with category in case category changed
        var updated = await partRepository.GetByIdWithCategoryAsync(partId);
        return ApiResponse<PartResponseDto>.SuccessResponse(MapToResponse(updated!), "Part updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int partId)
    {
        var part = await partRepository.GetByIdAsync(partId);
        if (part is null)
            return ApiResponse<bool>.FailureResponse("Part not found.");

        partRepository.Delete(part);
        await partRepository.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Part deleted successfully.");
    }

    // --- Private Mapping Helpers ---

    private static PartResponseDto MapToResponse(Part part) => new()
    {
        PartId = part.PartId,
        PartCategoryId = part.PartCategoryId,
        CategoryName = part.Category?.CategoryName ?? string.Empty,
        Sku = part.Sku,
        PartName = part.PartName,
        PartDescription = part.PartDescription,
        SellingPrice = part.SellingPrice,
        CurrentStock = part.CurrentStock,
        LowStockThreshold = part.LowStockThreshold,
        IsLowStock = part.CurrentStock <= part.LowStockThreshold,
        IsInStock = part.CurrentStock > 0,
        IsActive = part.IsActive
    };

    private static PartListItemDto MapToListItem(Part part) => new()
    {
        PartId = part.PartId,
        Sku = part.Sku,
        PartName = part.PartName,
        CategoryName = part.Category?.CategoryName ?? string.Empty,
        SellingPrice = part.SellingPrice,
        CurrentStock = part.CurrentStock,
        IsInStock = part.CurrentStock > 0,
        IsLowStock = part.CurrentStock <= part.LowStockThreshold,
        IsActive = part.IsActive
    };
}
