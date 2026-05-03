using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

/// <summary>
/// EF Core repository implementation for vendor-specific queries.
/// </summary>
public class VendorRepository(AppDbContext context)
    : RepositoryBase<Vendor>(context), IVendorRepository
{
    /// <inheritdoc />
    public IQueryable<Vendor> GetVendorQuery(bool trackChanges = false)
    {
        return trackChanges
            ? Context.Vendors
            : Context.Vendors.AsNoTracking();
    }
    
    
    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string vendorName, int? excludeVendorId = null)
    {
        var normalized = vendorName.Trim().ToLower();

        var query = Context.Vendors.AsNoTracking()
            .Where(v => v.VendorName.ToLower() == normalized);

        if (excludeVendorId.HasValue)
        {
            query = query.Where(v => v.VendorId != excludeVendorId.Value);
        }

        return await query.AnyAsync();
    }
    
    
    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(string vendorEmail, int? excludeVendorId = null)
    {
        var normalized = vendorEmail.Trim().ToLower();

        var query = Context.Vendors.AsNoTracking()
            .Where(v => v.VendorEmail.ToLower() == normalized);

        if (excludeVendorId.HasValue)
        {
            query = query.Where(v => v.VendorId != excludeVendorId.Value);
        }

        return await query.AnyAsync();
    }

    
    /// <inheritdoc />
    public async Task<bool> HasPurchaseHistoryAsync(int vendorId)
    {
        return await Context.Purchases.AsNoTracking().AnyAsync(p => p.VendorId == vendorId);
    }

    
    /// <inheritdoc />
    public IQueryable<Purchase> GetVendorPurchasesQuery(int vendorId, bool trackChanges = false)
    {
        var query = Context.Purchases.Where(p => p.VendorId == vendorId);

        return trackChanges
            ? query
            : query.AsNoTracking();
    }
}
