using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IReviewRepository : IRepositoryBase<Review>
{
    /// <summary>
    /// Checks whether a review already exists for the given service record.
    /// </summary>
    Task<bool> ExistsByServiceRecordIdAsync(int serviceRecordId);

    /// <summary>
    /// Checks whether the service record belongs to the provided user.
    /// </summary>
    Task<bool> IsServiceRecordOwnedByUserAsync(int serviceRecordId, string applicationUserId);

    /// <summary>
    /// Checks whether the target service record is completed for the provided user.
    /// </summary>
    Task<bool> IsCompletedServiceRecordForUserAsync(int serviceRecordId, string applicationUserId);

    /// <summary>
    /// Returns all reviews with related user and vehicle context.
    /// </summary>
    Task<List<Review>> GetAllWithDetailsAsync();

    /// <summary>
    /// Returns featured reviews with related user and vehicle context.
    /// </summary>
    Task<List<Review>> GetFeaturedAsync(int limit = 10);

    /// <summary>
    /// Returns one review with details.
    /// </summary>
    Task<Review?> GetByIdWithDetailsAsync(int reviewId);

    /// <summary>
    /// Returns one review constrained to customer scope.
    /// </summary>
    Task<Review?> GetByIdForUserAsync(int reviewId, string applicationUserId);
}
