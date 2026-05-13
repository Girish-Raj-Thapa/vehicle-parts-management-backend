using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Purchase;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Application.Services;

public class PurchaseService(
    IPurchaseRepository purchaseRepository,
    IVendorRepository vendorRepository,
    IPartRepository partRepository) : IPurchaseService
{
    public async Task<PagedResponse<List<PurchaseListItemDto>>> GetPagedAsync(PurchaseQueryDto query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 10 : query.PageSize;

        if (query.FromDate.HasValue && query.ToDate.HasValue && query.FromDate.Value > query.ToDate.Value)
        {
            return PagedFailure<PurchaseListItemDto>(
                "fromDate cannot be greater than toDate.",
                "dateRange");
        }

        DateTime? fromDate = query.FromDate.HasValue
            ? DateTime.SpecifyKind(query.FromDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
            : null;
        DateTime? toDate = query.ToDate.HasValue
            ? DateTime.SpecifyKind(query.ToDate.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc)
            : null;

        var (items, totalItems) = await purchaseRepository.GetPagedAsync(
            page,
            pageSize,
            query.VendorId,
            fromDate,
            toDate);

        var data = items.Select(MapListItem).ToList();
        return PagedResponse<List<PurchaseListItemDto>>.SuccessPagedResponse(
            data,
            new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
            },
            "Purchases retrieved successfully.");
    }


    public async Task<ApiResponse<PurchaseDetailDto>> GetByIdAsync(int purchaseId)
    {
        var purchase = await purchaseRepository.GetByIdWithDetailsAsync(purchaseId);
        return purchase is null
            ? ApiResponse<PurchaseDetailDto>.FailureResponse("Purchase not found.")
            : ApiResponse<PurchaseDetailDto>.SuccessResponse(MapDetail(purchase), "Purchase retrieved successfully.");
    }


    public async Task<ApiResponse<PurchaseCreateResponseDto>> CreateAsync(
        CreatePurchaseDto dto,
        string createdByApplicationUserId)
    {
        var vendor = await vendorRepository.GetByIdAsync(dto.VendorId);
        if (vendor is null || !vendor.IsActive)
        {
            return ApiResponse<PurchaseCreateResponseDto>.FailureResponse(
                "Vendor not found.",
                new Dictionary<string, List<string>>
                {
                    { "vendorId", ["Vendor not found or inactive."] }
                });
        }

        if (dto.Items.Count == 0)
        {
            return ApiResponse<PurchaseCreateResponseDto>.FailureResponse(
                "At least one purchase item is required.",
                new Dictionary<string, List<string>>
                {
                    { "items", ["At least one purchase item is required."] }
                });
        }

        var duplicatePartIds = dto.Items
            .GroupBy(i => i.PartId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicatePartIds.Count > 0)
        {
            return ApiResponse<PurchaseCreateResponseDto>.FailureResponse(
                "Duplicate part lines are not allowed in one purchase.",
                new Dictionary<string, List<string>>
                {
                    { "items", ["Duplicate part lines are not allowed in one purchase."] }
                });
        }

        var partIds = dto.Items.Select(i => i.PartId).Distinct().ToList();
        var parts = await partRepository.GetByIdsAsync(partIds);
        if (parts.Count != partIds.Count)
        {
            return ApiResponse<PurchaseCreateResponseDto>.FailureResponse(
                "One or more parts were not found.",
                new Dictionary<string, List<string>>
                {
                    { "items", ["One or more parts were not found."] }
                });
        }

        var partsById = parts.ToDictionary(p => p.PartId);
        var purchaseItems = dto.Items.Select(item =>
        {
            var lineTotal = Math.Round(
                item.QuantityPurchased * item.UnitPriceAtPurchase,
                2,
                MidpointRounding.AwayFromZero);

            return new PurchaseItem
            {
                PartId = item.PartId,
                QuantityPurchased = item.QuantityPurchased,
                UnitPriceAtPurchase = item.UnitPriceAtPurchase,
                PurchaseLineTotal = lineTotal
            };
        }).ToList();

        var purchaseDate = dto.PurchaseDate.HasValue
            ? DateTime.SpecifyKind(dto.PurchaseDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
            : DateTime.UtcNow;

        var purchase = new Purchase
        {
            VendorId = dto.VendorId,
            CreatedByApplicationUserId = createdByApplicationUserId,
            PurchaseInvoiceNo = $"PUR-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            PurchaseDate = purchaseDate,
            PurchaseStatus = PurchaseStatus.Completed,
            TotalAmount = purchaseItems.Sum(i => i.PurchaseLineTotal),
            PurchaseItems = purchaseItems
        };

        foreach (var item in dto.Items)
        {
            var part = partsById[item.PartId];
            part.CurrentStock += item.QuantityPurchased;
        }

        var stockMovements = dto.Items.Select(item => new StockMovement
        {
            PartId = item.PartId,
            MovementType = StockMovementType.In,
            MovementQuantity = item.QuantityPurchased,
            MovementDate = DateTime.UtcNow,
            ReferenceType = StockMovementReferenceType.Purchase,
            ReferenceId = 0
        }).ToList();

        await purchaseRepository.CreateInvoiceWithStockUpdatesAsync(purchase, parts, stockMovements);

        return ApiResponse<PurchaseCreateResponseDto>.SuccessResponse(
            new PurchaseCreateResponseDto
            {
                PurchaseId = purchase.PurchaseId,
                PurchaseInvoiceNo = purchase.PurchaseInvoiceNo,
                TotalAmount = purchase.TotalAmount,
                PurchaseStatus = purchase.PurchaseStatus.ToString()
            },
            "Purchase invoice created and stock updated successfully.");
    }


    private static PurchaseListItemDto MapListItem(Purchase purchase) => new()
    {
        PurchaseId = purchase.PurchaseId,
        PurchaseInvoiceNo = purchase.PurchaseInvoiceNo,
        VendorName = purchase.Vendor?.VendorName ?? string.Empty,
        PurchaseDate = DateOnly.FromDateTime(purchase.PurchaseDate),
        TotalAmount = purchase.TotalAmount,
        PurchaseStatus = purchase.PurchaseStatus.ToString()
    };


    private static PurchaseDetailDto MapDetail(Purchase purchase) => new()
    {
        PurchaseId = purchase.PurchaseId,
        PurchaseInvoiceNo = purchase.PurchaseInvoiceNo,
        PurchaseDate = DateOnly.FromDateTime(purchase.PurchaseDate),
        Vendor = new PurchaseVendorDto
        {
            VendorId = purchase.VendorId,
            VendorName = purchase.Vendor?.VendorName ?? string.Empty
        },
        CreatedBy = new PurchaseCreatedByDto
        {
            UserId = purchase.CreatedByApplicationUserId,
            FullName = purchase.CreatedByUser?.FullName ?? string.Empty
        },
        Items = purchase.PurchaseItems
            .OrderBy(i => i.PurchaseItemId)
            .Select(i => new PurchaseItemDto
            {
                PurchaseItemId = i.PurchaseItemId,
                PartId = i.PartId,
                PartName = i.Part?.PartName ?? string.Empty,
                Sku = i.Part?.Sku ?? string.Empty,
                QuantityPurchased = i.QuantityPurchased,
                UnitPriceAtPurchase = i.UnitPriceAtPurchase,
                PurchaseLineTotal = i.PurchaseLineTotal
            })
            .ToList(),
        TotalAmount = purchase.TotalAmount,
        PurchaseStatus = purchase.PurchaseStatus.ToString()
    };


    private static PagedResponse<List<T>> PagedFailure<T>(string message, string errorKey) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = new Dictionary<string, List<string>> { { errorKey, [message] } },
            Data = new List<T>(),
            Pagination = new PaginationMeta()
        };
}
