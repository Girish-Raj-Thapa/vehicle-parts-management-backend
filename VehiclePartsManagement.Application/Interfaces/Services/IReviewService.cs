using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Review;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IReviewService
{
    Task<ApiResponse<ReviewCreateResponseDto>> CreateAsync(
        CreateReviewDto dto,
        string applicationUserId);

    Task<ApiResponse<List<ReviewListItemDto>>> GetAllAsync();

    Task<ApiResponse<List<FeaturedReviewListItemDto>>> GetFeaturedAsync(int limit = 10);

    Task<ApiResponse<ReviewDetailDto>> GetByIdAsync(int reviewId, string? applicationUserId);
}
