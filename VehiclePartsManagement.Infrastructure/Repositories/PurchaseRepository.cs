using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class PurchaseRepository(AppDbContext context)
    : RepositoryBase<Purchase>(context), IPurchaseRepository
{
    public async Task<(List<Purchase> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        int? vendorId,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var query = Context.Purchases
            .AsNoTracking()
            .Include(p => p.Vendor)
            .AsQueryable();

        if (vendorId.HasValue)
        {
            query = query.Where(p => p.VendorId == vendorId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(p => p.PurchaseDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(p => p.PurchaseDate <= toDate.Value);
        }

        var totalItems = await query.CountAsync();
        if (totalItems == 0)
        {
            return ([], 0);
        }

        var items = await query
            .OrderByDescending(p => p.PurchaseDate)
            .ThenByDescending(p => p.PurchaseId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }


    public Task<Purchase?> GetByIdWithDetailsAsync(int purchaseId) =>
        Context.Purchases
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Include(p => p.CreatedByUser)
            .Include(p => p.PurchaseItems)
                .ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);


    public async Task CreateInvoiceWithStockUpdatesAsync(
        Purchase purchase,
        IEnumerable<Part> partsToUpdate,
        IEnumerable<StockMovement> stockMovements)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();

        Context.Purchases.Add(purchase);
        await Context.SaveChangesAsync();

        var movements = stockMovements.ToList();
        foreach (var movement in movements)
        {
            movement.ReferenceId = purchase.PurchaseId;
        }

        Context.Parts.UpdateRange(partsToUpdate);
        Context.StockMovements.AddRange(movements);

        await Context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
