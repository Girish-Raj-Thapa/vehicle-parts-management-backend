using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class VehicleRepository(AppDbContext context)
    : RepositoryBase<Vehicle>(context), IVehicleRepository
{
    /// <summary>
    /// Retrieves vehicles with optional owner-scope and search filtering plus pagination.
    /// </summary>
    public async Task<(List<Vehicle> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        string? applicationUserId,
        string? search)
    {
        var query = Context.Vehicles
            .AsNoTracking()
            .Include(v => v.User)
            .Include(v => v.Model)
            .ThenInclude(m => m.Brand)
            .Include(v => v.VehicleImages)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(applicationUserId))
        {
            query = query.Where(v => v.ApplicationUserId == applicationUserId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(v =>
                v.VehicleNumber.ToLower().Contains(term) ||
                v.Model.ModelName.ToLower().Contains(term) ||
                v.Model.Brand.BrandName.ToLower().Contains(term));
        }

        var totalItems = await query.CountAsync();
        if (totalItems == 0)
        {
            return ([], 0);
        }

        var items = await query
            .OrderByDescending(v => v.VehicleRegisteredAt)
            .ThenByDescending(v => v.VehicleId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }


    /// <summary>
    /// Retrieves full vehicle detail with owner, model, brand, images, and related counts.
    /// </summary>
    public Task<Vehicle?> GetByIdWithDetailsAsync(int vehicleId) =>
        Context.Vehicles
            .AsNoTracking()
            .Include(v => v.User)
            .Include(v => v.Model)
            .ThenInclude(m => m.Brand)
            .Include(v => v.VehicleImages.OrderBy(i => i.DisplayOrder))
            .Include(v => v.ServiceRecords)
            .Include(v => v.Sales)
            .Include(v => v.PartRequests)
            .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);


    /// <summary>
    /// Retrieves full vehicle detail constrained to a single customer scope.
    /// </summary>
    public Task<Vehicle?> GetByIdForUserAsync(int vehicleId, string applicationUserId) =>
        Context.Vehicles
            .AsNoTracking()
            .Include(v => v.User)
            .Include(v => v.Model)
            .ThenInclude(m => m.Brand)
            .Include(v => v.VehicleImages.OrderBy(i => i.DisplayOrder))
            .Include(v => v.ServiceRecords)
            .Include(v => v.Sales)
            .Include(v => v.PartRequests)
            .FirstOrDefaultAsync(v =>
                v.VehicleId == vehicleId &&
                v.ApplicationUserId == applicationUserId);


    /// <summary>
    /// Returns paged service records for a vehicle, including reviews.
    /// </summary>
    public async Task<(List<ServiceRecord> Items, int TotalItems)> GetServiceHistoryAsync(
        int vehicleId,
        int page,
        int pageSize)
    {
        var query = Context.ServiceRecords
            .AsNoTracking()
            .Include(sr => sr.Review)
            .Where(sr => sr.VehicleId == vehicleId);

        var totalItems = await query.CountAsync();
        if (totalItems == 0)
        {
            return ([], 0);
        }

        var items = await query
            .OrderByDescending(sr => sr.ServiceDate)
            .ThenByDescending(sr => sr.ServiceRecordId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }


    /// <summary>
    /// Returns paged sales for a vehicle, including sale items with part details.
    /// </summary>
    public async Task<(List<Sale> Items, int TotalItems)> GetSaleHistoryAsync(
        int vehicleId,
        int page,
        int pageSize)
    {
        var query = Context.Sales
            .AsNoTracking()
            .Include(s => s.ProcessedByUser)
            .Include(s => s.SaleItems)
            .ThenInclude(si => si.Part)
            .Where(s => s.VehicleId == vehicleId);

        var totalItems = await query.CountAsync();
        if (totalItems == 0)
        {
            return ([], 0);
        }

        var items = await query
            .OrderByDescending(s => s.SaleDate)
            .ThenByDescending(s => s.SaleId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }


    /// <summary>
    /// Verifies vehicle ownership for authorization checks.
    /// </summary>
    public Task<bool> IsVehicleOwnedByUserAsync(int vehicleId, string applicationUserId) =>
        Context.Vehicles
            .AsNoTracking()
            .AnyAsync(v => v.VehicleId == vehicleId && v.ApplicationUserId == applicationUserId);
}
