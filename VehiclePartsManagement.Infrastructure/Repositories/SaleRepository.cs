using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class SaleRepository(AppDbContext context)
    : RepositoryBase<Sale>(context), ISaleRepository
{
    public async Task<(List<Sale> Items, int TotalItems)> GetCustomerPurchaseHistoryAsync(
        string customerUserId,
        int page,
        int pageSize,
        PaymentStatus? paymentStatus,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var query = Context.Sales
            .AsNoTracking()
            .Include(s => s.Vehicle)
            .ThenInclude(v => v.Model)
            .ThenInclude(m => m.Brand)
            .Include(s => s.Payments)
            .Where(s =>
                s.CustomerApplicationUserId == customerUserId &&
                s.SaleStatus != SaleStatus.Draft &&
                s.SaleStatus != SaleStatus.Cancelled);
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

<<<<<<< HEAD
        if (fromDate.HasValue)
        {
            query = query.Where(s => s.SaleDate.Date >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(s => s.SaleDate.Date <= toDate.Value.Date);
        }

=======
        // Count query is executed separately for pagination metadata.
>>>>>>> prashish
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


<<<<<<< HEAD
    public Task<Sale?> GetCustomerPurchaseDetailAsync(string customerUserId, int saleId) =>
        Context.Sales
            .AsNoTracking()
            .Include(s => s.Vehicle)
            .ThenInclude(v => v.Model)
            .ThenInclude(m => m.Brand)
            .Include(s => s.SaleItems)
            .ThenInclude(si => si.Part)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s =>
                s.SaleId == saleId &&
                s.CustomerApplicationUserId == customerUserId &&
                s.SaleStatus != SaleStatus.Draft &&
                s.SaleStatus != SaleStatus.Cancelled);
    public async Task<Sale?> GetSaleWithDetailsAsync(int saleId)
    {
        return await Context.Sales
            .Include(s => s.CustomerUser)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.SaleId == saleId);
    }

    public async Task<IEnumerable<Sale>> GetOverdueSalesAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await Context.Sales
            .Include(s => s.CustomerUser)
            .Where(s => s.BalanceDue > 0 && s.DueDate < today)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetAllSalesAsync()
    {
        return await Context.Sales
            .Include(s => s.CustomerUser)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }
=======
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
>>>>>>> prashish
}
