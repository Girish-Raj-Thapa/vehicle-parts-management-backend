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

        var utcFromDate = fromDate.HasValue
            ? DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc)
            : (DateTime?)null;
        var utcToDateExclusive = toDate.HasValue
            ? DateTime.SpecifyKind(toDate.Value.Date.AddDays(1), DateTimeKind.Utc)
            : (DateTime?)null;

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.SaleDate >= utcFromDate!.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(s => s.SaleDate < utcToDateExclusive!.Value);
        }

        if (paymentStatus.HasValue)
        {
            query = query.Where(s => s.PaymentStatus == paymentStatus.Value);
        }

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
    /// Returns one sale with full detail used by sales detail and invoice views.
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
    /// Returns one sale with full detail constrained to a specific customer scope.
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
