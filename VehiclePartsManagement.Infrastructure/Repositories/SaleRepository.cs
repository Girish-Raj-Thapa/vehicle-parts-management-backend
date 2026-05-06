using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class SaleRepository(AppDbContext context)
    : RepositoryBase<Sale>(context), ISaleRepository
{
    /// <summary>
    /// Retrieves sales with optional customer and payment-status filtering and pagination.
    /// </summary>
    public async Task<(List<Sale> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        string? customerApplicationUserId,
        PaymentStatus? paymentStatus)
    {
        var query = Context.Sales
            .AsNoTracking()
            .Include(s => s.CustomerUser)
            .Include(s => s.Vehicle)
            .Include(s => s.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(customerApplicationUserId))
        {
            query = query.Where(s => s.CustomerApplicationUserId == customerApplicationUserId);
        }

        if (paymentStatus.HasValue)
        {
            query = query.Where(s => s.PaymentStatus == paymentStatus.Value);
        }

        // Count query is executed separately for pagination metadata.
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
    /// Retrieves a single sale with related customer, vehicle, items, part, and payment data.
    /// </summary>
    public Task<Sale?> GetByIdWithDetailsAsync(int saleId) =>
        Context.Sales
            .AsNoTracking()
            .Include(s => s.CustomerUser)
            .Include(s => s.Vehicle)
                .ThenInclude(v => v.Model)
                .ThenInclude(m => m.Brand)
            .Include(s => s.SaleItems)
                .ThenInclude(i => i.Part)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.SaleId == saleId);


    /// <summary>
    /// Retrieves a single sale with full detail constrained to one customer scope.
    /// </summary>
    public Task<Sale?> GetByIdForCustomerAsync(int saleId, string customerApplicationUserId) =>
        Context.Sales
            .AsNoTracking()
            .Include(s => s.CustomerUser)
            .Include(s => s.Vehicle)
                .ThenInclude(v => v.Model)
                .ThenInclude(m => m.Brand)
            .Include(s => s.SaleItems)
                .ThenInclude(i => i.Part)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s =>
                s.SaleId == saleId &&
                s.CustomerApplicationUserId == customerApplicationUserId);
}
