using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.PartCategory;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Services;

public class PartCategoryService(IPartCategoryRepository categoryRepository) : IPartCategoryService
{
    public async Task<ApiResponse<List<PartCategoryResponseDto>>> GetAllAsync()
    {
        var categories = await categoryRepository.GetAllOrderedAsync();
        var dtos = categories.Select(MapToResponse).ToList();
        return ApiResponse<List<PartCategoryResponseDto>>.SuccessResponse(dtos, "Categories retrieved successfully.");
    }

    public async Task<ApiResponse<PartCategoryResponseDto>> CreateAsync(CreatePartCategoryDto dto)
    {
        if (await categoryRepository.ExistsByNameAsync(dto.CategoryName))
            return ApiResponse<PartCategoryResponseDto>.FailureResponse("A category with this name already exists.");

        var category = new PartCategory
        {
            CategoryName = dto.CategoryName.Trim(),
            CategoryDescription = dto.CategoryDescription?.Trim() ?? string.Empty
        };

        categoryRepository.Create(category);
        await categoryRepository.SaveChangesAsync();

        return ApiResponse<PartCategoryResponseDto>.SuccessResponse(MapToResponse(category), "Category created successfully.");
    }

    public async Task<ApiResponse<PartCategoryResponseDto>> UpdateAsync(int categoryId, UpdatePartCategoryDto dto)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId);
        if (category is null)
            return ApiResponse<PartCategoryResponseDto>.FailureResponse("Category not found.");

        if (await categoryRepository.ExistsByNameAsync(dto.CategoryName, categoryId))
            return ApiResponse<PartCategoryResponseDto>.FailureResponse("A category with this name already exists.");

        category.CategoryName = dto.CategoryName.Trim();
        category.CategoryDescription = dto.CategoryDescription?.Trim() ?? string.Empty;

        categoryRepository.Update(category);
        await categoryRepository.SaveChangesAsync();

        return ApiResponse<PartCategoryResponseDto>.SuccessResponse(MapToResponse(category), "Category updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int categoryId)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId);
        if (category is null)
            return ApiResponse<bool>.FailureResponse("Category not found.");

        if (await categoryRepository.HasLinkedPartsAsync(categoryId))
            return ApiResponse<bool>.FailureResponse("Cannot delete category because it has linked parts.");

        categoryRepository.Delete(category);
        await categoryRepository.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully.");
    }

    private static PartCategoryResponseDto MapToResponse(PartCategory category) => new()
    {
        CategoryId = category.PartCategoryId,
        CategoryName = category.CategoryName,
        CategoryDescription = category.CategoryDescription
    };
}
