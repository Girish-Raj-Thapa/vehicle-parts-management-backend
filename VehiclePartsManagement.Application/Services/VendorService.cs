using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Vendor;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Services;

/// <summary>
/// Implements vendor-management business rules and response shaping.
/// </summary>
public class VendorService(IVendorRepository vendorRepository) : IVendorService
{
    /// <inheritdoc />
    public async Task<PagedResponse<List<VendorListItemDto>>> GetVendorsAsync(VendorListQueryDto query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        
        var vendorQuery = vendorRepository.GetVendorQuery(trackChanges: false);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            vendorQuery = vendorRepository.SearchVendors(vendorQuery, term);
        }
        
        if (query.IsActive.HasValue)
        {
            vendorQuery = vendorQuery.Where(v => v.IsActive == query.IsActive.Value);
        }
        
        var totalItems = await vendorQuery.CountAsync();
        
        var items = await vendorQuery
            .OrderBy(v => v.VendorName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VendorListItemDto
            {
                VendorId = v.VendorId,
                VendorName = v.VendorName,
                ContactPerson = v.ContactPerson,
                VendorEmail = v.VendorEmail,
                VendorPhone = v.VendorPhone,
                VendorAddress = v.VendorAddress,
                IsActive = v.IsActive
            })
            .ToListAsync();
        
        var pagination = new PaginationMeta
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };

        return PagedResponse<List<VendorListItemDto>>.SuccessPagedResponse(
            items,
            pagination,
            "Vendors retrieved successfully.");
    }


    /// <inheritdoc />
    public async Task<ApiResponse<VendorDetailsDto>> GetVendorByIdAsync(int vendorId)
    {
        var vendor = await vendorRepository.GetByIdAsync(vendorId);

        if (vendor == null)
        {
            return ApiResponse<VendorDetailsDto>.FailureResponse(
                "Vendor details not found.",
                new Dictionary<string, List<string>>
                {
                    ["vendorId"] = ["Vendor does not exist."]
                });
        }
        
        return ApiResponse<VendorDetailsDto>.SuccessResponse(
            MapVendorDetails(vendor),
            "Vendor retrieved successfully.");
    }


    /// <inheritdoc />
    public async Task<ApiResponse<VendorDetailsDto>> CreateVendorAsync(CreateVendorDto dto)
    {
        var duplicateErrors = await ValidateUniqueFieldsAsync(dto.VendorName, dto.VendorEmail, excludeVendorId: null);
        if (duplicateErrors.Count > 0)
        {
            return ApiResponse<VendorDetailsDto>.FailureResponse("Validation failed.", duplicateErrors);
        }

        var vendor = new Vendor
        {
            VendorName = dto.VendorName.Trim(),
            ContactPerson = dto.ContactPerson.Trim(),
            VendorEmail = dto.VendorEmail.Trim(),
            VendorPhone = dto.VendorPhone.Trim(),
            VendorAddress = dto.VendorAddress.Trim(),
            IsActive = dto.IsActive
        };
        
        vendorRepository.Create(vendor);
        await vendorRepository.SaveChangesAsync();
        
        return ApiResponse<VendorDetailsDto>.SuccessResponse(
            MapVendorDetails(vendor),
            "Vendor Created Successfully."
        );
    }

    /// <inheritdoc />
    public async Task<ApiResponse<VendorDetailsDto>> UpdateVendorAsync(int vendorId, UpdateVendorDto dto)
    {
        var vendor = await vendorRepository.GetByIdAsync(vendorId);

        if (vendor is null)
        {
            return ApiResponse<VendorDetailsDto>.FailureResponse(
                "Vendor not found.",
                new Dictionary<string, List<string>>
                {
                    ["vendorId"] = ["Vendor does not exist."]
                }
            );
        }
        
        var duplicateErrors = await ValidateUniqueFieldsAsync(dto.VendorName, dto.VendorEmail, vendorId);
        if (duplicateErrors.Count > 0)
        {
            return ApiResponse<VendorDetailsDto>.FailureResponse("Validation failed.", duplicateErrors);
        }
        
        vendor.VendorName = dto.VendorName.Trim();
        vendor.ContactPerson = dto.ContactPerson.Trim();
        vendor.VendorEmail = dto.VendorEmail.Trim();
        vendor.VendorPhone = dto.VendorPhone.Trim();
        vendor.VendorAddress = dto.VendorAddress.Trim();
        vendor.IsActive = dto.IsActive;
        
        vendorRepository.Update(vendor);
        await vendorRepository.SaveChangesAsync();

        return ApiResponse<VendorDetailsDto>.SuccessResponse(
            MapVendorDetails(vendor),
            "Vendor updated successfully."
        );
    }

    /// <inheritdoc />
    public async Task<ApiResponse> UpdateVendorStatusAsync(int vendorId, UpdateVendorStatusDto dto)
    {
        if (!dto.IsActive.HasValue)
        {
            return ApiResponse.FailureResponse(
                "Validation failed.",
                new Dictionary<string, List<string>>
                {
                    ["isActive"] = ["The isActive field is required."]
                }
            );
        }

        var vendor = await vendorRepository.GetByIdAsync(vendorId);
        if (vendor is null)
        {
            return ApiResponse.FailureResponse(
                "Vendor not found.",
                new Dictionary<string, List<string>>
                {
                    ["vendorId"] = ["Vendor does not exist."]
                }
            );
        }

        var isActive = dto.IsActive.Value;
        vendor.IsActive = isActive;

        vendorRepository.Update(vendor);
        await vendorRepository.SaveChangesAsync();

        return ApiResponse.SuccessResponse($"Vendor {(isActive ? "activated" : "deactivated")} successfully.");
    }

    /// <inheritdoc />
    public async Task<ApiResponse> DeleteVendorAsync(int vendorId)
    {
        var vendor = await vendorRepository.GetByIdAsync(vendorId);
        if (vendor is null)
        {
            return ApiResponse.FailureResponse(
                "Vendor not found.",
                new Dictionary<string, List<string>>
                {
                    ["vendorId"] = ["Vendor does not exist."]
                });
        }

        var hasHistory = await vendorRepository.HasPurchaseHistoryAsync(vendorId);
        if (hasHistory)
        {
            return ApiResponse.FailureResponse(
                "Vendor cannot be deleted because purchase history exists. Please deactivate the vendor instead.",
                new Dictionary<string, List<string>>
                {
                    ["vendorId"] = ["Vendor has linked purchase records."]
                }
            );
        }

        vendorRepository.Delete(vendor);
        await vendorRepository.SaveChangesAsync();

        return ApiResponse.SuccessResponse("Vendor deleted successfully.");
    }

    /// <inheritdoc />
    public async Task<PagedResponse<List<VendorPurchaseItemDto>>> GetVendorPurchasesAsync(
        int vendorId,
        int page,
        int pageSize,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var vendorExists = await vendorRepository.FindByCondition(v => v.VendorId == vendorId).AnyAsync();
        if (!vendorExists)
        {
            return new PagedResponse<List<VendorPurchaseItemDto>>
            {
                Success = false,
                Message = "Vendor not found.",
                Errors = new Dictionary<string, List<string>>
                {
                    ["vendorId"] = ["Vendor does not exist."]
                },
                Data = null,
                Pagination = new PaginationMeta { Page = page, PageSize = pageSize, TotalItems = 0, TotalPages = 0 }
            };
        }

        if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
        {
            return new PagedResponse<List<VendorPurchaseItemDto>>
            {
                Success = false,
                Message = "Invalid date range.",
                Errors = new Dictionary<string, List<string>>
                {
                    ["dateRange"] = ["fromDate cannot be greater than toDate."]
                },
                Data = null,
                Pagination = new PaginationMeta { Page = page, PageSize = pageSize, TotalItems = 0, TotalPages = 0 }
            };
        }

        var safePage = page <= 0 ? 1 : page;
        var safePageSize = pageSize <= 0 ? 10 : pageSize;

        var purchasesQuery = vendorRepository.GetVendorPurchasesQuery(vendorId, trackChanges: false);

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.Date;
            purchasesQuery = purchasesQuery.Where(p => p.PurchaseDate >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.Date.AddDays(1).AddTicks(-1);
            purchasesQuery = purchasesQuery.Where(p => p.PurchaseDate <= to);
        }

        var totalItems = await purchasesQuery.CountAsync();

        var items = await purchasesQuery
            .OrderByDescending(p => p.PurchaseDate)
            .ThenByDescending(p => p.PurchaseId)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(p => new VendorPurchaseItemDto
            {
                PurchaseId = p.PurchaseId,
                PurchaseInvoiceNo = p.PurchaseInvoiceNo,
                PurchaseDate = p.PurchaseDate,
                PurchaseStatus = p.PurchaseStatus.ToString(),
                TotalAmount = p.TotalAmount
            })
            .ToListAsync();

        var pagination = new PaginationMeta
        {
            Page = safePage,
            PageSize = safePageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)safePageSize)
        };

        return PagedResponse<List<VendorPurchaseItemDto>>.SuccessPagedResponse(
            items,
            pagination,
            "Vendor purchase history retrieved successfully.");
    }

    /// <inheritdoc />
    public async Task<ApiResponse<VendorSummaryDto>> GetVendorSummaryAsync(int vendorId)
    {
        var vendor = await vendorRepository.GetByIdAsync(vendorId);
        if (vendor is null)
        {
            return ApiResponse<VendorSummaryDto>.FailureResponse(
                "Vendor not found.",
                new Dictionary<string, List<string>>
                {
                    ["vendorId"] = ["Vendor does not exist."]
                });
        }

        var purchaseData = await vendorRepository.GetVendorPurchasesQuery(vendorId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalPurchaseInvoices = g.Count(),
                TotalPurchaseAmount = g.Sum(x => x.TotalAmount),
                LastPurchaseDate = g.Max(x => (DateTime?)x.PurchaseDate)
            })
            .FirstOrDefaultAsync();

        var summary = new VendorSummaryDto
        {
            VendorId = vendor.VendorId,
            VendorName = vendor.VendorName,
            TotalPurchaseInvoices = purchaseData?.TotalPurchaseInvoices ?? 0,
            TotalPurchaseAmount = Math.Round(purchaseData?.TotalPurchaseAmount ?? 0m, 2),
            LastPurchaseDate = purchaseData?.LastPurchaseDate,
            IsActive = vendor.IsActive
        };

        return ApiResponse<VendorSummaryDto>.SuccessResponse(summary, "Vendor summary retrieved successfully.");
    }
    
    
    // Private Helper Functions
    private async Task<Dictionary<string, List<string>>> ValidateUniqueFieldsAsync(
        string vendorName,
        string vendorEmail,
        int? excludeVendorId)
    {
        var errors = new Dictionary<string, List<string>>();

        if (await vendorRepository.ExistsByNameAsync(vendorName, excludeVendorId))
        {
            errors["vendorName"] = ["A vendor with the same name already exists."];
        }

        if (await vendorRepository.ExistsByEmailAsync(vendorEmail, excludeVendorId))
        {
            errors["vendorEmail"] = ["A vendor with the same email already exists."];
        }

        return errors;
    }

    private static VendorDetailsDto MapVendorDetails(Vendor vendor)
    {
        return new VendorDetailsDto
        {
            VendorId = vendor.VendorId,
            VendorName = vendor.VendorName,
            ContactPerson = vendor.ContactPerson,
            VendorEmail = vendor.VendorEmail,
            VendorPhone = vendor.VendorPhone,
            VendorAddress = vendor.VendorAddress,
            IsActive = vendor.IsActive
        };
    }
}
