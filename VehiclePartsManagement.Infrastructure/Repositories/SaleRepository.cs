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

        if (paymentStatus.HasValue)
        {
            query = query.Where(s => s.PaymentStatus == paymentStatus.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.SaleDate.Date >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(s => s.SaleDate.Date <= toDate.Value.Date);
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
}
