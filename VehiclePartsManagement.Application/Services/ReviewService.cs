using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Review;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Services;

public class ReviewService(IReviewRepository reviewRepository) : IReviewService
{
    /// <summary>
    /// Submits a customer review for a service record after ownership and duplicate checks.
    /// </summary>
    public async Task<ApiResponse<ReviewCreateResponseDto>> CreateAsync(
        CreateReviewDto dto,
        string applicationUserId)
    {
        if (dto.Rating is < 1 or > 5)
        {
            return ApiResponse<ReviewCreateResponseDto>.FailureResponse(
                "Rating must be between 1 and 5.",
                new Dictionary<string, List<string>>
                {
                    { "rating", ["Rating must be between 1 and 5."] }
                });
        }

        if (!await reviewRepository.IsServiceRecordOwnedByUserAsync(dto.ServiceRecordId, applicationUserId))
        {
            return ApiResponse<ReviewCreateResponseDto>.FailureResponse(
                "Service record not found for the current user.",
                new Dictionary<string, List<string>>
                {
                    { "serviceRecordId", ["Service record not found for the current user."] }
                });
        }

        if (!await reviewRepository.IsCompletedServiceRecordForUserAsync(dto.ServiceRecordId, applicationUserId))
        {
            return ApiResponse<ReviewCreateResponseDto>.FailureResponse(
                "Review can only be submitted for completed services.",
                new Dictionary<string, List<string>>
                {
                    { "serviceRecordId", ["Review can only be submitted for completed services."] }
                });
        }

        if (await reviewRepository.ExistsByServiceRecordIdAsync(dto.ServiceRecordId))
        {
            return ApiResponse<ReviewCreateResponseDto>.FailureResponse(
                "A review already exists for this service record.",
                new Dictionary<string, List<string>>
                {
                    { "serviceRecordId", ["A review already exists for this service record."] }
                });
        }

        var review = new Review
        {
            ServiceRecordId = dto.ServiceRecordId,
            ApplicationUserId = applicationUserId,
            Rating = dto.Rating,
            ReviewComment = dto.ReviewComment.Trim(),
            ReviewDate = DateTime.UtcNow
        };

        reviewRepository.Create(review);
        await reviewRepository.SaveChangesAsync();

        return ApiResponse<ReviewCreateResponseDto>.SuccessResponse(
            new ReviewCreateResponseDto
            {
                ReviewId = review.ReviewId
            },
            "Review submitted successfully");
    }


    /// <summary>
    /// Returns all reviews with customer-facing summary fields.
    /// </summary>
    public async Task<ApiResponse<List<ReviewListItemDto>>> GetAllAsync()
    {
        var reviews = await reviewRepository.GetAllWithDetailsAsync();
        var data = reviews
            .Select(MapListItem)
            .ToList();

        return ApiResponse<List<ReviewListItemDto>>.SuccessResponse(
            data,
            "Reviews retrieved successfully");
    }


    /// <summary>
    /// Returns featured reviews ordered by rating and recency.
    /// </summary>
    public async Task<ApiResponse<List<FeaturedReviewListItemDto>>> GetFeaturedAsync(int limit = 10)
    {
        var safeLimit = limit < 1 ? 10 : limit;
        var reviews = await reviewRepository.GetFeaturedAsync(safeLimit);
        var data = reviews
            .Select(MapFeaturedItem)
            .ToList();

        return ApiResponse<List<FeaturedReviewListItemDto>>.SuccessResponse(
            data,
            "Featured reviews retrieved successfully");
    }


    /// <summary>
    /// Returns one review detail, optionally constrained to customer ownership.
    /// </summary>
    public async Task<ApiResponse<ReviewDetailDto>> GetByIdAsync(int reviewId, string? applicationUserId)
    {
        var review = string.IsNullOrWhiteSpace(applicationUserId)
            ? await reviewRepository.GetByIdWithDetailsAsync(reviewId)
            : await reviewRepository.GetByIdForUserAsync(reviewId, applicationUserId);

        if (review is null)
        {
            return ApiResponse<ReviewDetailDto>.FailureResponse("Review not found.");
        }

        return ApiResponse<ReviewDetailDto>.SuccessResponse(
            MapDetailItem(review),
            "Review retrieved successfully");
    }


    private static ReviewListItemDto MapListItem(Review review) =>
        new()
        {
            ReviewId = review.ReviewId,
            CustomerName = review.User.FullName,
            Rating = review.Rating,
            ReviewComment = review.ReviewComment,
            ReviewDate = DateOnly.FromDateTime(review.ReviewDate)
        };


    private static FeaturedReviewListItemDto MapFeaturedItem(Review review) =>
        new()
        {
            ReviewId = review.ReviewId,
            CustomerName = review.User.FullName,
            VehicleName = $"{review.ServiceRecord.Vehicle.Model.Brand.BrandName} {review.ServiceRecord.Vehicle.Model.ModelName}",
            Rating = review.Rating,
            ReviewComment = review.ReviewComment
        };


    private static ReviewDetailDto MapDetailItem(Review review) =>
        new()
        {
            ReviewId = review.ReviewId,
            ServiceRecordId = review.ServiceRecordId,
            CustomerName = review.User.FullName,
            VehicleName = $"{review.ServiceRecord.Vehicle.Model.Brand.BrandName} {review.ServiceRecord.Vehicle.Model.ModelName}",
            Rating = review.Rating,
            ReviewComment = review.ReviewComment,
            ReviewDate = DateOnly.FromDateTime(review.ReviewDate)
        };
}
