using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

/// <summary>
/// Vendor-specific data-access contract.
/// </summary>
public interface IVendorRepository : IRepositoryBase<Vendor>
{
    /// <summary>
    /// Returns the base vendor query with optional tracking.
    /// </summary>
    IQueryable<Vendor> GetVendorQuery(bool trackChanges = false);

    /// <summary>
    /// Checks whether another vendor already uses the same name.
    /// </summary>
    Task<bool> ExistsByNameAsync(string vendorName, int? excludeVendorId = null);

    /// <summary>
    /// Checks whether another vendor already uses the same email.
    /// </summary>
    Task<bool> ExistsByEmailAsync(string vendorEmail, int? excludeVendorId = null);

    /// <summary>
    /// Returns true when at least one purchase is linked to the vendor.
    /// </summary>
    Task<bool> HasPurchaseHistoryAsync(int vendorId);

    /// <summary>
    /// Returns purchase query for a given vendor with optional tracking.
    /// </summary>
    IQueryable<Purchase> GetVendorPurchasesQuery(int vendorId, bool trackChanges = false);

    /// <summary>
    /// Returns a vendor query filtered by a search term (name, contact, email, phone).
    /// </summary>
    IQueryable<Vendor> SearchVendors(IQueryable<Vendor> query, string searchTerm);
}
