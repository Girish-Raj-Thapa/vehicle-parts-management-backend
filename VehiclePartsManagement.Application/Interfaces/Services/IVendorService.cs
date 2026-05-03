using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Vendor;

namespace VehiclePartsManagement.Application.Interfaces.Services;

/// <summary>
/// Exposes vendor-management operations for admin workflows.
/// </summary>
public interface IVendorService
{
    /// <summary>
    /// Returns paginated vendors with optional search and active-status filter.
    /// </summary>
    Task<PagedResponse<List<VendorListItemDto>>> GetVendorsAsync(VendorListQueryDto query);

    /// <summary>
    /// Returns vendor details by identifier.
    /// </summary>
    Task<ApiResponse<VendorDetailsDto>> GetVendorByIdAsync(int vendorId);

    /// <summary>
    /// Creates a new vendor after duplicate checks.
    /// </summary>
    Task<ApiResponse<VendorDetailsDto>> CreateVendorAsync(CreateVendorDto dto);

    /// <summary>
    /// Updates an existing vendor record.
    /// </summary>
    Task<ApiResponse<VendorDetailsDto>> UpdateVendorAsync(int vendorId, UpdateVendorDto dto);

    /// <summary>
    /// Activates or deactivates a vendor.
    /// </summary>
    Task<ApiResponse> UpdateVendorStatusAsync(int vendorId, UpdateVendorStatusDto dto);

    /// <summary>
    /// Deletes a vendor only when purchase history does not exist.
    /// </summary>
    Task<ApiResponse> DeleteVendorAsync(int vendorId);

    /// <summary>
    /// Returns paginated purchase history for a specific vendor.
    /// </summary>
    Task<PagedResponse<List<VendorPurchaseItemDto>>> GetVendorPurchasesAsync(
        int vendorId,
        int page,
        int pageSize,
        DateTime? fromDate,
        DateTime? toDate);

    /// <summary>
    /// Returns vendor-level purchase summary metrics.
    /// </summary>
    Task<ApiResponse<VendorSummaryDto>> GetVendorSummaryAsync(int vendorId);
}
