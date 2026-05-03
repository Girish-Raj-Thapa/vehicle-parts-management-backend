using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class PartRequestRepository(AppDbContext context)
    : RepositoryBase<PartRequest>(context), IPartRequestRepository
{
    /// <summary>
    /// Retrieves part requests with optional owner/status filtering and pagination.
    /// </summary>
    public async Task<(List<PartRequest> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        string? applicationUserId,
        PartRequestStatus? status)
    {
        var query = Context.PartRequests
            .AsNoTracking()
            .Include(pr => pr.User)
            .Include(pr => pr.Vehicle)
            .ThenInclude(v => v!.Model)
            .ThenInclude(m => m.Brand)
            .Include(pr => pr.FulfilledPart)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(applicationUserId))
        {
            query = query.Where(pr => pr.ApplicationUserId == applicationUserId);
        }

        if (status.HasValue)
        {
            query = query.Where(pr => pr.RequestStatus == status.Value);
        }

        // Count query is executed separately for pagination metadata.
        var totalItems = await query.CountAsync();
        if (totalItems == 0)
        {
            return ([], 0);
        }

        var items = await query
            .OrderByDescending(pr => pr.RequestDate)
            .ThenByDescending(pr => pr.PartRequestId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }


    /// <summary>
    /// Retrieves one part request detail with related customer/vehicle/fulfilled-part data.
    /// </summary>
    public Task<PartRequest?> GetByIdWithDetailsAsync(int partRequestId) =>
        Context.PartRequests
            .AsNoTracking()
            .Include(pr => pr.User)
            .Include(pr => pr.Vehicle)
            .ThenInclude(v => v!.Model)
            .ThenInclude(m => m.Brand)
            .Include(pr => pr.FulfilledPart)
            .FirstOrDefaultAsync(pr => pr.PartRequestId == partRequestId);


    /// <summary>
    /// Retrieves one part request detail constrained to an owner scope.
    /// </summary>
    public Task<PartRequest?> GetByIdForUserAsync(int partRequestId, string applicationUserId) =>
        Context.PartRequests
            .AsNoTracking()
            .Include(pr => pr.User)
            .Include(pr => pr.Vehicle)
            .ThenInclude(v => v!.Model)
            .ThenInclude(m => m.Brand)
            .Include(pr => pr.FulfilledPart)
            .FirstOrDefaultAsync(pr =>
                pr.PartRequestId == partRequestId &&
                pr.ApplicationUserId == applicationUserId);


    /// <summary>
    /// Verifies vehicle ownership for customer request creation checks.
    /// </summary>
    public Task<bool> IsVehicleOwnedByUserAsync(int vehicleId, string applicationUserId) =>
        Context.Vehicles
            .AsNoTracking()
            .AnyAsync(v => v.VehicleId == vehicleId && v.ApplicationUserId == applicationUserId);


    /// <summary>
    /// Verifies fulfilled-part foreign key validity for staff/admin updates.
    /// </summary>
    public Task<bool> FulfilledPartExistsAsync(int partId) =>
        Context.Parts
            .AsNoTracking()
            .AnyAsync(p => p.PartId == partId);
}
