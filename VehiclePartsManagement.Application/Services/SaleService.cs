using Microsoft.AspNetCore.Identity;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Sale;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Application.Services;

/// <summary>
/// Handles sales creation and sales read workflows, including pricing, payment state,
/// inventory movement, and DTO mapping for API responses.
/// </summary>
public class SaleService(
    ISaleRepository saleRepository,
    IPaymentRepository paymentRepository,
    IPartRepository partRepository,
    IVehicleRepository vehicleRepository,
    IStockMovementRepository stockMovementRepository,
    UserManager<ApplicationUser> userManager) : ISaleService
{
    private const decimal LoyaltyDiscountPercent = 10m;
    private const decimal LoyaltyThresholdAmount = 5000m;

    /// <summary>
    /// Creates a sale, decrements inventory stock, and optionally records initial payment.
    /// </summary>
    public async Task<ApiResponse<SaleCreateResponseDto>> CreateAsync(
        CreateSaleDto dto,
        string processedByApplicationUserId)
    {
        // Customer must exist and belong to Customer role for sale ownership integrity.
        var customer = await userManager.FindByIdAsync(dto.CustomerUserId);
        if (customer is null || !await userManager.IsInRoleAsync(customer, "Customer"))
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Customer not found.",
                new Dictionary<string, List<string>>
                {
                    { "customerUserId", ["Customer not found."] }
                });
        }

        // Vehicle must exist and belong to the same selected customer.
        var vehicle = await vehicleRepository.GetByIdAsync(dto.VehicleId);
        if (vehicle is null)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Vehicle not found.",
                new Dictionary<string, List<string>>
                {
                    { "vehicleId", ["Vehicle not found."] }
                });
        }

        if (vehicle.ApplicationUserId != dto.CustomerUserId)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Selected vehicle does not belong to the selected customer.",
                new Dictionary<string, List<string>>
                {
                    { "vehicleId", ["Selected vehicle does not belong to the selected customer."] }
                });
        }

        // Sale requires at least one line item.
        if (dto.Items.Count == 0)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "At least one sale item is required.",
                new Dictionary<string, List<string>>
                {
                    { "items", ["At least one sale item is required."] }
                });
        }

        // Duplicate part rows are blocked to keep line-level arithmetic deterministic.
        var duplicatePartIds = dto.Items
            .GroupBy(i => i.PartId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicatePartIds.Count > 0)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Duplicate part lines are not allowed in one sale.",
                new Dictionary<string, List<string>>
                {
                    { "items", ["Duplicate part lines are not allowed in one sale."] }
                });
        }

        // Batch-load parts to validate all references in a single repository call.
        var partIds = dto.Items.Select(i => i.PartId).Distinct().ToList();
        var parts = await partRepository.GetByIdsAsync(partIds);
        if (parts.Count != partIds.Count)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "One or more parts were not found.",
                new Dictionary<string, List<string>>
                {
                    { "items", ["One or more parts were not found."] }
                });
        }

        var partsById = parts.ToDictionary(p => p.PartId);

        // Validate inventory constraints before any write operation is executed.
        foreach (var line in dto.Items)
        {
            var part = partsById[line.PartId];
            if (!part.IsActive)
            {
                return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                    $"Part '{part.PartName}' is inactive.",
                    new Dictionary<string, List<string>>
                    {
                        { "items", [$"Part '{part.PartName}' is inactive."] }
                    });
            }

            if (part.CurrentStock < line.Quantity)
            {
                return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                    $"Insufficient stock for part '{part.PartName}'.",
                    new Dictionary<string, List<string>>
                    {
                        { "items", [$"Insufficient stock for part '{part.PartName}'."] }
                    });
            }
        }

        // Price snapshot is taken at sale time for invoice/history consistency.
        var saleItems = dto.Items.Select(line =>
        {
            var part = partsById[line.PartId];
            var lineTotal = part.SellingPrice * line.Quantity;

            return new SaleItem
            {
                PartId = line.PartId,
                QuantitySold = line.Quantity,
                UnitPriceAtSale = part.SellingPrice,
                LineTotal = lineTotal
            };
        }).ToList();

        // Loyalty discount rule: 10% only when subtotal is strictly greater than threshold.
        var subTotal = saleItems.Sum(i => i.LineTotal);
        var discountPercent = subTotal > LoyaltyThresholdAmount ? LoyaltyDiscountPercent : 0m;
        var discountAmount = Math.Round((subTotal * discountPercent) / 100m, 2, MidpointRounding.AwayFromZero);
        var totalAmount = subTotal - discountAmount;

        // Credit sale and due-date combination is validated to prevent inconsistent states.
        if (dto.IsCreditSale && !dto.DueDate.HasValue)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Due date is required for credit sales.",
                new Dictionary<string, List<string>>
                {
                    { "dueDate", ["Due date is required for credit sales."] }
                });
        }

        if (!dto.IsCreditSale && dto.DueDate.HasValue)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Due date is only allowed for credit sales.",
                new Dictionary<string, List<string>>
                {
                    { "dueDate", ["Due date is only allowed for credit sales."] }
                });
        }

        // Payment amount validation enforces no-negative and no-overpayment rules.
        var paymentAmount = dto.Payment?.PaymentAmount ?? 0m;
        if (paymentAmount < 0m)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Payment amount cannot be negative.",
                new Dictionary<string, List<string>>
                {
                    { "payment.paymentAmount", ["Payment amount cannot be negative."] }
                });
        }

        if (paymentAmount > totalAmount)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Payment amount cannot exceed total amount.",
                new Dictionary<string, List<string>>
                {
                    { "payment.paymentAmount", ["Payment amount cannot exceed total amount."] }
                });
        }

        // Optional payment method is parsed only when payment payload is provided.
        PaymentMethodType? paymentMethod = null;
        if (dto.Payment is not null)
        {
            if (!Enum.TryParse<PaymentMethodType>(dto.Payment.PaymentMethod, true, out var parsedMethod))
            {
                return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                    "Payment method is invalid.",
                    new Dictionary<string, List<string>>
                    {
                        { "payment.paymentMethod", ["Payment method is invalid."] }
                    });
            }

            paymentMethod = parsedMethod;
        }

        var balanceDue = totalAmount - paymentAmount;
        var paymentStatus = ResolvePaymentStatus(balanceDue, dto.DueDate);

        var sale = new Sale
        {
            CustomerApplicationUserId = dto.CustomerUserId,
            VehicleId = dto.VehicleId,
            ProcessedByApplicationUserId = processedByApplicationUserId,
            SaleInvoiceNo = $"SAL-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            SaleDate = DateTime.UtcNow,
            SaleStatus = SaleStatus.Completed,
            SubTotal = subTotal,
            LoyaltyDiscountPercent = discountPercent,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            BalanceDue = balanceDue,
            PaymentStatus = paymentStatus,
            DueDate = dto.DueDate.HasValue
                ? DateTime.SpecifyKind(dto.DueDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
                : null,
            SaleItems = saleItems
        };

        // Persist sale first to obtain SaleId for payment and stock movement references.
        saleRepository.Create(sale);
        await saleRepository.SaveChangesAsync();

        // Initial payment is optional and recorded only when amount is positive.
        if (dto.Payment is not null && paymentAmount > 0m && paymentMethod.HasValue)
        {
            var payment = new Payment
            {
                SaleId = sale.SaleId,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = paymentMethod.Value,
                PaymentAmount = paymentAmount,
                PaymentReference = dto.Payment.PaymentReference.Trim(),
                PaymentNote = dto.Payment.PaymentNote.Trim()
            };

            paymentRepository.Create(payment);
        }

        // Inventory is decremented per line and each decrement is audited in StockMovement.
        foreach (var line in dto.Items)
        {
            var part = partsById[line.PartId];
            part.CurrentStock -= line.Quantity;
            partRepository.Update(part);

            stockMovementRepository.Create(new StockMovement
            {
                PartId = part.PartId,
                MovementType = StockMovementType.Out,
                MovementQuantity = line.Quantity,
                MovementDate = DateTime.UtcNow,
                ReferenceType = StockMovementReferenceType.Sale,
                ReferenceId = sale.SaleId
            });
        }

        // Final save persists payment (if any), stock updates, and stock movements.
        await saleRepository.SaveChangesAsync();

        return ApiResponse<SaleCreateResponseDto>.SuccessResponse(
            new SaleCreateResponseDto
            {
                SaleId = sale.SaleId,
                SaleInvoiceNo = sale.SaleInvoiceNo,
                SubTotal = sale.SubTotal,
                LoyaltyDiscountPercent = sale.LoyaltyDiscountPercent,
                DiscountAmount = sale.DiscountAmount,
                TotalAmount = sale.TotalAmount,
                PaidAmount = paymentAmount,
                BalanceDue = sale.BalanceDue,
                PaymentStatus = MapPaymentStatus(sale.PaymentStatus)
            },
            "Sale invoice created successfully");
    }


    /// <summary>
    /// Retrieves paged sales with optional customer and payment-status filters.
    /// </summary>
    public async Task<PagedResponse<List<SaleListItemDto>>> GetPagedAsync(
        SaleListQueryDto query,
        string? customerScopeUserId)
    {
        // Normalize pagination limits to keep endpoint behavior predictable.
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 10 : query.PageSize;

        PaymentStatus? paymentStatus = null;
        if (!string.IsNullOrWhiteSpace(query.PaymentStatus))
        {
            if (!TryParsePaymentStatus(query.PaymentStatus, out var parsedStatus))
            {
                return PagedFailure("Payment status is invalid.", "paymentStatus");
            }

            paymentStatus = parsedStatus;
        }

        // Customer-scoped reads override explicit query customer filter for authorization safety.
        var customerFilter = !string.IsNullOrWhiteSpace(customerScopeUserId)
            ? customerScopeUserId
            : query.CustomerUserId;

        var (items, totalItems) = await saleRepository.GetPagedAsync(page, pageSize, customerFilter, paymentStatus);
        var data = items.Select(MapListItem).ToList();

        return PagedResponse<List<SaleListItemDto>>.SuccessPagedResponse(
            data,
            new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            },
            "Sales retrieved successfully");
    }


    /// <summary>
    /// Retrieves one sale detail, optionally constrained to a specific customer scope.
    /// </summary>
    public async Task<ApiResponse<SaleDetailDto>> GetByIdAsync(int saleId, string? customerScopeUserId)
    {
        var sale = string.IsNullOrWhiteSpace(customerScopeUserId)
            ? await saleRepository.GetByIdWithDetailsAsync(saleId)
            : await saleRepository.GetByIdForCustomerAsync(saleId, customerScopeUserId);

        if (sale is null)
        {
            return ApiResponse<SaleDetailDto>.FailureResponse("Sale not found.");
        }

        return ApiResponse<SaleDetailDto>.SuccessResponse(
            MapDetail(sale),
            "Sale retrieved successfully");
    }

    /// <summary>
    /// Returns invoice-focused sale payload, optionally constrained to customer scope.
    /// </summary>
    public async Task<ApiResponse<SaleInvoiceDto>> GetInvoiceAsync(int saleId, string? customerScopeUserId)
    {
        var sale = string.IsNullOrWhiteSpace(customerScopeUserId)
            ? await saleRepository.GetByIdWithDetailsAsync(saleId)
            : await saleRepository.GetByIdForCustomerAsync(saleId, customerScopeUserId);

        if (sale is null)
        {
            return ApiResponse<SaleInvoiceDto>.FailureResponse("Sale invoice not found.");
        }

        return ApiResponse<SaleInvoiceDto>.SuccessResponse(
            MapInvoice(sale),
            "Invoice retrieved successfully");
    }


    private static PaymentStatus ResolvePaymentStatus(decimal balanceDue, DateOnly? dueDate)
    {
        if (balanceDue <= 0m)
        {
            return PaymentStatus.Paid;
        }

        if (dueDate.HasValue && dueDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return PaymentStatus.Overdue;
        }

        return PaymentStatus.PartiallyPaid;
    }


    private static bool TryParsePaymentStatus(string value, out PaymentStatus paymentStatus)
    {
        if (Enum.TryParse<PaymentStatus>(value, true, out paymentStatus))
        {
            return true;
        }

        if (string.Equals(value, "Partial", StringComparison.OrdinalIgnoreCase))
        {
            paymentStatus = PaymentStatus.PartiallyPaid;
            return true;
        }

        if (string.Equals(value, "Unpaid", StringComparison.OrdinalIgnoreCase))
        {
            paymentStatus = PaymentStatus.Unpaid;
            return true;
        }

        return false;
    }


    private static string MapPaymentStatus(PaymentStatus paymentStatus) =>
        paymentStatus switch
        {
            PaymentStatus.PartiallyPaid => "Partial",
            _ => paymentStatus.ToString()
        };


    private static SaleListItemDto MapListItem(Sale sale)
    {
        var paidAmount = sale.Payments.Sum(p => p.PaymentAmount);
        return new SaleListItemDto
        {
            SaleId = sale.SaleId,
            SaleInvoiceNo = sale.SaleInvoiceNo,
            CustomerName = sale.CustomerUser.FullName,
            SaleDate = DateOnly.FromDateTime(sale.SaleDate),
            TotalAmount = sale.TotalAmount,
            PaidAmount = paidAmount,
            BalanceDue = sale.BalanceDue,
            PaymentStatus = MapPaymentStatus(sale.PaymentStatus),
            DueDate = sale.DueDate.HasValue ? DateOnly.FromDateTime(sale.DueDate.Value) : null
        };
    }


    private static SaleDetailDto MapDetail(Sale sale)
    {
        var paidAmount = sale.Payments.Sum(p => p.PaymentAmount);
        return new SaleDetailDto
        {
            SaleId = sale.SaleId,
            SaleInvoiceNo = sale.SaleInvoiceNo,
            SaleDate = DateOnly.FromDateTime(sale.SaleDate),
            Customer = new SaleDetailCustomerDto
            {
                UserId = sale.CustomerApplicationUserId,
                FullName = sale.CustomerUser.FullName,
                Email = sale.CustomerUser.Email ?? string.Empty,
                PhoneNumber = sale.CustomerUser.PhoneNumber ?? string.Empty
            },
            Vehicle = new SaleDetailVehicleDto
            {
                VehicleId = sale.VehicleId,
                VehicleNumber = sale.Vehicle.VehicleNumber,
                BrandName = sale.Vehicle.Model.Brand.BrandName,
                ModelName = sale.Vehicle.Model.ModelName
            },
            Items = sale.SaleItems.Select(i => new SaleDetailItemDto
            {
                PartId = i.PartId,
                PartName = i.Part.PartName,
                Quantity = i.QuantitySold,
                UnitPriceAtSale = i.UnitPriceAtSale,
                LineTotal = i.LineTotal
            }).ToList(),
            SubTotal = sale.SubTotal,
            LoyaltyDiscountPercent = sale.LoyaltyDiscountPercent,
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount,
            PaidAmount = paidAmount,
            BalanceDue = sale.BalanceDue,
            PaymentStatus = MapPaymentStatus(sale.PaymentStatus),
            DueDate = sale.DueDate.HasValue ? DateOnly.FromDateTime(sale.DueDate.Value) : null
        };
    }

    private static SaleInvoiceDto MapInvoice(Sale sale)
    {
        var paidAmount = sale.Payments.Sum(p => p.PaymentAmount);
        return new SaleInvoiceDto
        {
            InvoiceNo = sale.SaleInvoiceNo,
            InvoiceDate = DateOnly.FromDateTime(sale.SaleDate),
            CustomerName = sale.CustomerUser.FullName,
            CustomerEmail = sale.CustomerUser.Email ?? string.Empty,
            Vehicle = $"{sale.Vehicle.Model.Brand.BrandName} {sale.Vehicle.Model.ModelName} - {sale.Vehicle.VehicleNumber}",
            Items = sale.SaleItems.Select(i => new SaleInvoiceItemDto
            {
                PartName = i.Part.PartName,
                Quantity = i.QuantitySold,
                UnitPrice = i.UnitPriceAtSale,
                LineTotal = i.LineTotal
            }).ToList(),
            SubTotal = sale.SubTotal,
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount,
            PaidAmount = paidAmount,
            BalanceDue = sale.BalanceDue
        };
    }


    private static PagedResponse<List<SaleListItemDto>> PagedFailure(string message, string errorKey) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = new Dictionary<string, List<string>>
            {
                { errorKey, [message] }
            },
            Data = new List<SaleListItemDto>(),
            Pagination = new PaginationMeta()
        };
}
