using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.DTOs.Part;
using VehiclePartsManagement.Application.Interfaces;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Services;

public class PartService(
    IPartRepository partRepository,
    IImageStorageService imageStorageService) : IPartService
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
    public async Task<PagedResponse<List<PartListItemDto>>> GetAllAsync(PartQueryDto query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 12 : query.PageSize;

        var partQuery = partRepository.GetPartsQuery(trackChanges: false);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            partQuery = partRepository.SearchParts(partQuery, query.Search.Trim());
        }

        if (query.CategoryId.HasValue)
        {
            partQuery = partQuery.Where(p => p.PartCategoryId == query.CategoryId.Value);
        }

        if (query.ModelId.HasValue)
        {
            partQuery = partQuery.Where(p => p.PartCompatibilities.Any(pc => pc.VehicleModelId == query.ModelId.Value));
        }

        if (query.InStock.HasValue && query.InStock.Value)
        {
            partQuery = partQuery.Where(p => p.CurrentStock > 0);
        }

        if (query.IsActive.HasValue)
        {
            partQuery = partQuery.Where(p => p.IsActive == query.IsActive.Value);
        }
        else
        {
            partQuery = partQuery.Where(p => p.IsActive);
        }

        var totalItems = await partQuery.CountAsync();

        var items = await partQuery
            .OrderBy(p => p.PartName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToListItem).ToList();

        var pagination = new PaginationMeta
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };

        return PagedResponse<List<PartListItemDto>>.SuccessPagedResponse(dtos, pagination, "Parts retrieved successfully.");
    }

    public async Task<ApiResponse<PartResponseDto>> GetByIdAsync(int partId)
    {
        var part = await partRepository.GetByIdWithDetailsAsync(partId);
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
            PartDescription = dto.PartDescription ?? string.Empty,
            SellingPrice = dto.SellingPrice,
            CurrentStock = dto.CurrentStock,
            LowStockThreshold = dto.LowStockThreshold,
            IsActive = true
        };

        partRepository.Create(part);
        await partRepository.SaveChangesAsync();

        var created = await partRepository.GetByIdWithDetailsAsync(part.PartId);
        if (created == null) return ApiResponse<PartResponseDto>.FailureResponse("Failed to reload part after creation.");
        
        return ApiResponse<PartResponseDto>.SuccessResponse(MapToResponse(created), "Part created successfully.");
    }

    public async Task<ApiResponse<PartResponseDto>> UpdateAsync(int partId, UpdatePartDto dto)
    {
        var part = await partRepository.GetByIdWithDetailsAsync(partId);
        if (part is null)
            return ApiResponse<PartResponseDto>.FailureResponse("Part not found.");

        if (dto.PartCategoryId.HasValue) part.PartCategoryId = dto.PartCategoryId.Value;
        if (dto.Sku is not null) part.Sku = dto.Sku;
        if (dto.PartName is not null) part.PartName = dto.PartName;
        if (dto.PartDescription is not null) part.PartDescription = dto.PartDescription;
        if (dto.SellingPrice.HasValue) part.SellingPrice = dto.SellingPrice.Value;
        if (dto.LowStockThreshold.HasValue) part.LowStockThreshold = dto.LowStockThreshold.Value;
        if (dto.IsActive.HasValue) part.IsActive = dto.IsActive.Value;
        if (dto.CurrentStock.HasValue) part.CurrentStock = dto.CurrentStock.Value;

        partRepository.Update(part);
        await partRepository.SaveChangesAsync();

        var updated = await partRepository.GetByIdWithDetailsAsync(partId);
        return ApiResponse<PartResponseDto>.SuccessResponse(MapToResponse(updated!), "Part updated successfully.");
    }

    public async Task<ApiResponse<PartStatusResponseDto>> DeactivateAsync(int partId, PartDeactivateDto dto)
    {
        var part = await partRepository.GetByIdAsync(partId);
        if (part is null)
            return ApiResponse<PartStatusResponseDto>.FailureResponse("Part not found.");

        part.IsActive = dto.IsActive;
        partRepository.Update(part);
        await partRepository.SaveChangesAsync();

        return ApiResponse<PartStatusResponseDto>.SuccessResponse(
            new PartStatusResponseDto { PartId = partId, IsActive = part.IsActive },
            "Part status updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int partId)
    {
        var part = await partRepository.GetByIdAsync(partId);
        if (part is null)
            return ApiResponse<bool>.FailureResponse("Part not found.");

        if (!part.IsActive)
            return ApiResponse<bool>.FailureResponse("Part is already deleted.");

        // Soft delete per spec: one-way deactivation through DELETE.
        part.IsActive = false;
        partRepository.Update(part);
        await partRepository.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Part deleted successfully.");
    }

    public async Task<ApiResponse<List<PartFeaturedItemDto>>> GetFeaturedAsync()
    {
        var parts = await partRepository.GetFeaturedPartsAsync();
        var dtos = parts.Select(p => new PartFeaturedItemDto
        {
            PartId = p.PartId,
            PartName = p.PartName,
            CategoryName = p.Category?.CategoryName ?? string.Empty,
            SellingPrice = p.SellingPrice,
            CurrentStock = p.CurrentStock,
            Tag = "HOT",
            CompatibleModels = p.PartCompatibilities
                .Select(pc => $"{pc.Model.Brand.BrandName} {pc.Model.ModelName}")
                .ToList()
        }).ToList();

        return ApiResponse<List<PartFeaturedItemDto>>.SuccessResponse(dtos, "Featured parts retrieved successfully.");
    }

    public async Task<ApiResponse<List<PartListItemDto>>> GetRelatedAsync(int partId)
    {
        var part = await partRepository.GetByIdAsync(partId);
        if (part is null)
            return ApiResponse<List<PartListItemDto>>.FailureResponse("Part not found.");

        var relatedParts = await partRepository.GetRelatedPartsAsync(partId, part.PartCategoryId);
        var dtos = relatedParts.Select(MapToListItem).ToList();

        return ApiResponse<List<PartListItemDto>>.SuccessResponse(dtos, "Related parts retrieved successfully.");
    }

    public async Task<ApiResponse<PartImageDto>> UploadImageAsync(
        int partId,
        Stream fileStream,
        string fileName,
        string contentType,
        long length,
        bool isPrimary,
        int displayOrder)
    {
        var part = await partRepository.GetByIdWithDetailsAsync(partId);
        if (part is null)
            return ApiResponse<PartImageDto>.FailureResponse("Part not found.");

        // If the new image is primary, mark other images of this part as not primary
        if (isPrimary)
        {
            foreach (var img in part.PartImages)
            {
                img.IsPrimary = false;
            }
        }
        else if (part.PartImages.Count == 0)
        {
            // First image is primary by default if not specified otherwise
            isPrimary = true;
        }

        string imagePath;
        try
        {
            imagePath = await imageStorageService.SaveImageAsync(
                fileStream,
                fileName,
                contentType,
                length,
                "parts");
        }
        catch (ArgumentException ex)
        {
            return ApiResponse<PartImageDto>.FailureResponse(ex.Message);
        }

        var partImage = new PartImage
        {
            PartId = partId,
            ImagePath = imagePath,
            IsPrimary = isPrimary,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };

        part.PartImages.Add(partImage);
        partRepository.Update(part);
        await partRepository.SaveChangesAsync();

        var dto = new PartImageDto
        {
            PartImageId = partImage.PartImageId,
            ImagePath = partImage.ImagePath,
            IsPrimary = partImage.IsPrimary,
            DisplayOrder = partImage.DisplayOrder
        };

        return ApiResponse<PartImageDto>.SuccessResponse(dto, "Image uploaded successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteImageAsync(int partId, int imageId)
    {
        var part = await partRepository.GetByIdWithDetailsAsync(partId);
        if (part is null)
            return ApiResponse<bool>.FailureResponse("Part not found.");

        var partImage = part.PartImages.FirstOrDefault(pi => pi.PartImageId == imageId);
        if (partImage is null)
            return ApiResponse<bool>.FailureResponse("Image not found on this part.");

        // Delete from storage
        await imageStorageService.DeleteImageAsync(partImage.ImagePath);

        // Delete from database
        part.PartImages.Remove(partImage);
        partRepository.Update(part);
        await partRepository.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Image deleted successfully.");
    }

    public async Task<ApiResponse<PartImageDto>> SetPrimaryImageAsync(int partId, int imageId)
    {
        var part = await partRepository.GetByIdWithDetailsAsync(partId);
        if (part is null)
            return ApiResponse<PartImageDto>.FailureResponse("Part not found.");

        var targetImage = part.PartImages.FirstOrDefault(pi => pi.PartImageId == imageId);
        if (targetImage is null)
            return ApiResponse<PartImageDto>.FailureResponse("Image not found on this part.");

        foreach (var img in part.PartImages)
        {
            img.IsPrimary = (img.PartImageId == imageId);
        }

        partRepository.Update(part);
        await partRepository.SaveChangesAsync();

        var dto = new PartImageDto
        {
            PartImageId = targetImage.PartImageId,
            ImagePath = targetImage.ImagePath,
            IsPrimary = targetImage.IsPrimary,
            DisplayOrder = targetImage.DisplayOrder
        };

        return ApiResponse<PartImageDto>.SuccessResponse(dto, "Primary image set successfully.");
    }

    // --- Private Mapping Helpers ---

    private static PartResponseDto MapToResponse(Part part) => new()
    {
        PartId = part.PartId,
        Sku = part.Sku ?? string.Empty,
        PartName = part.PartName ?? string.Empty,
        PartDescription = part.PartDescription ?? string.Empty,
        PartCategoryId = part.PartCategoryId,
        CategoryName = part.Category?.CategoryName ?? string.Empty,
        SellingPrice = part.SellingPrice,
        CurrentStock = part.CurrentStock,
        LowStockThreshold = part.LowStockThreshold,
        IsLowStock = part.CurrentStock <= part.LowStockThreshold,
        IsInStock = part.CurrentStock > 0,
        IsActive = part.IsActive,
        CompatibleVehicles = part.PartCompatibilities?.Select(pc => new PartCompatibleVehicleDto
        {
            BrandId = pc.Model?.VehicleBrandId ?? 0,
            BrandName = pc.Model?.Brand?.BrandName ?? string.Empty,
            ModelId = pc.VehicleModelId,
            ModelName = pc.Model?.ModelName ?? string.Empty
        }).ToList() ?? new List<PartCompatibleVehicleDto>(),
        Images = part.PartImages?.Select(pi => new PartImageDto
        {
            PartImageId = pi.PartImageId,
            ImagePath = pi.ImagePath,
            IsPrimary = pi.IsPrimary,
            DisplayOrder = pi.DisplayOrder
        }).OrderBy(pi => pi.DisplayOrder).ToList() ?? new List<PartImageDto>()
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
        IsActive = part.IsActive,
        CompatibleModels = part.PartCompatibilities?
            .Where(pc => pc.Model != null && pc.Model.Brand != null)
            .Select(pc => $"{pc.Model.Brand.BrandName} {pc.Model.ModelName}")
            .ToList() ?? new List<string>(),
        PrimaryImageUrl = part.PartImages?.FirstOrDefault(pi => pi.IsPrimary)?.ImagePath 
            ?? part.PartImages?.OrderBy(pi => pi.DisplayOrder).FirstOrDefault()?.ImagePath
    };
}
