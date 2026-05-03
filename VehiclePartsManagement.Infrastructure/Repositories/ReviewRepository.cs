using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class ReviewRepository(AppDbContext context)
    : RepositoryBase<Review>(context), IReviewRepository
{
    /// <summary>
    /// Verifies one-review-per-service-record constraint.
    /// </summary>
    public Task<bool> ExistsByServiceRecordIdAsync(int serviceRecordId) =>
        Context.Reviews
            .AsNoTracking()
            .AnyAsync(r => r.ServiceRecordId == serviceRecordId);


    /// <summary>
    /// Verifies that the review target service record belongs to the current user.
    /// </summary>
    public Task<bool> IsServiceRecordOwnedByUserAsync(int serviceRecordId, string applicationUserId) =>
        Context.ServiceRecords
            .AsNoTracking()
            .AnyAsync(sr =>
                sr.ServiceRecordId == serviceRecordId &&
                sr.ServiceAppointment.ApplicationUserId == applicationUserId);


    /// <summary>
    /// Returns all reviews with customer and vehicle relationship graph loaded.
    /// </summary>
    public Task<List<Review>> GetAllWithDetailsAsync() =>
        Context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.ServiceRecord)
            .ThenInclude(sr => sr.Vehicle)
            .ThenInclude(v => v.Model)
            .ThenInclude(m => m.Brand)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();


    /// <summary>
    /// Returns featured reviews prioritized by highest ratings and newest records.
    /// </summary>
    public Task<List<Review>> GetFeaturedAsync(int limit = 10) =>
        Context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.ServiceRecord)
            .ThenInclude(sr => sr.Vehicle)
            .ThenInclude(v => v.Model)
            .ThenInclude(m => m.Brand)
            .OrderByDescending(r => r.Rating)
            .ThenByDescending(r => r.ReviewDate)
            .Take(limit)
            .ToListAsync();
}
