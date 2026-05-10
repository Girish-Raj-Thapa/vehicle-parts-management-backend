using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Sale;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Application.Services;

public class SaleService(
    ISaleRepository saleRepository,
    IPartRepository partRepository,
    IVehicleRepository vehicleRepository) : ISaleService
{
    public async Task<ApiResponse<SaleResponseDto>> CreateAsync(CreateSaleDto dto, string processedByUserId)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(dto.VehicleId);
        if (vehicle == null || vehicle.ApplicationUserId != dto.CustomerApplicationUserId)
        {
            return ApiResponse<SaleResponseDto>.FailureResponse("Invalid vehicle or customer.");
        }

        var sale = new Sale
        {
            CustomerApplicationUserId = dto.CustomerApplicationUserId,
            VehicleId = dto.VehicleId,
            ProcessedByApplicationUserId = processedByUserId,
            SaleInvoiceNo = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
            SaleDate = DateTime.UtcNow,
            SaleStatus = SaleStatus.Completed,
            PaymentStatus = PaymentStatus.Unpaid
        };

        decimal subTotal = 0;

        foreach (var itemDto in dto.Items)
        {
            var part = await partRepository.GetByIdAsync(itemDto.PartId);
            if (part == null || !part.IsActive)
            {
                return ApiResponse<SaleResponseDto>.FailureResponse($"Part with ID {itemDto.PartId} not found or inactive.");
            }

            if (part.CurrentStock < itemDto.Quantity)
            {
                return ApiResponse<SaleResponseDto>.FailureResponse($"Insufficient stock for part {part.PartName}. Available: {part.CurrentStock}.");
            }

            var lineTotal = part.SellingPrice * itemDto.Quantity;
            subTotal += lineTotal;

            part.CurrentStock -= itemDto.Quantity;
            partRepository.Update(part);

            sale.SaleItems.Add(new SaleItem
            {
                PartId = part.PartId,
                QuantitySold = itemDto.Quantity,
                UnitPriceAtSale = part.SellingPrice,
                LineTotal = lineTotal
            });
        }

        sale.SubTotal = subTotal;

        // Loyalty program: 10% discount for orders over 5000
        if (subTotal > 5000)
        {
            sale.LoyaltyDiscountPercent = 10;
        }
        else
        {
            sale.LoyaltyDiscountPercent = 0;
        }

        sale.DiscountAmount = sale.SubTotal * (sale.LoyaltyDiscountPercent / 100m);
        sale.TotalAmount = sale.SubTotal - sale.DiscountAmount;
        sale.BalanceDue = sale.TotalAmount;

        saleRepository.Create(sale);
        await saleRepository.SaveChangesAsync();

        return ApiResponse<SaleResponseDto>.SuccessResponse(new SaleResponseDto
        {
            SaleId = sale.SaleId,
            SaleInvoiceNo = sale.SaleInvoiceNo,
            SubTotal = sale.SubTotal,
            LoyaltyDiscountPercent = sale.LoyaltyDiscountPercent,
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount
        }, "Sale created successfully.");
    }
}
