using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Cart;
using VehiclePartsManagement.Application.DTOs.Part;
using VehiclePartsManagement.Application.DTOs.Sale;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Services;

public class CartService(
    ICartRepository cartRepository,
    ICartItemRepository cartItemRepository,
    IPartService partService,
    ISaleService saleService) : ICartService
{
    private const decimal LoyaltyDiscountPercent = 10m;
    private const decimal LoyaltyThresholdAmount = 5000m;

    public async Task<ApiResponse<CartDto>> GetMyCartAsync(string customerUserId)
    {
        var cart = await cartRepository.GetOrCreateByCustomerWithItemsAsync(customerUserId, trackChanges: false);
        return ApiResponse<CartDto>.SuccessResponse(MapCart(cart), "Cart retrieved successfully.");
    }

    public async Task<ApiResponse<CartDto>> AddItemAsync(string customerUserId, AddCartItemDto dto)
    {
        var cart = await cartRepository.GetOrCreateByCustomerWithItemsAsync(customerUserId, trackChanges: true);
        var partResponse = await partService.GetByIdAsync(dto.PartId);
        var partData = partResponse.Data as PartResponseDto;
        if (!partResponse.Success || partData is null)
        {
            return ApiResponse<CartDto>.FailureResponse("Part not found.");
        }

        if (!partData.IsActive)
        {
            return ApiResponse<CartDto>.FailureResponse($"Part '{partData.PartName}' is inactive.");
        }

        var existing = cart.CartItems.FirstOrDefault(x => x.PartId == dto.PartId);
        var targetQty = (existing?.Quantity ?? 0) + dto.Quantity;
        if (targetQty > partData.CurrentStock)
        {
            return ApiResponse<CartDto>.FailureResponse($"Insufficient stock for part '{partData.PartName}'.");
        }

        if (existing is null)
        {
            cart.CartItems.Add(new CartItem
            {
                PartId = dto.PartId,
                Quantity = dto.Quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Quantity = targetQty;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        cartRepository.Update(cart);
        await cartRepository.SaveChangesAsync();

        var refreshed = await cartRepository.GetOrCreateByCustomerWithItemsAsync(customerUserId, trackChanges: false);
        return ApiResponse<CartDto>.SuccessResponse(MapCart(refreshed), "Item added to cart successfully.");
    }

    public async Task<ApiResponse<CartDto>> UpdateItemAsync(string customerUserId, int cartItemId, UpdateCartItemDto dto)
    {
        var cart = await cartRepository.GetOrCreateByCustomerWithItemsAsync(customerUserId, trackChanges: true);
        var item = cart.CartItems.FirstOrDefault(x => x.CartItemId == cartItemId);
        if (item is null)
        {
            return ApiResponse<CartDto>.FailureResponse("Cart item not found.");
        }

        var partResponse = await partService.GetByIdAsync(item.PartId);
        var partData = partResponse.Data as PartResponseDto;
        if (!partResponse.Success || partData is null)
        {
            return ApiResponse<CartDto>.FailureResponse("Part not found.");
        }

        if (!partData.IsActive)
        {
            return ApiResponse<CartDto>.FailureResponse($"Part '{partData.PartName}' is inactive.");
        }

        if (dto.Quantity > partData.CurrentStock)
        {
            return ApiResponse<CartDto>.FailureResponse($"Insufficient stock for part '{partData.PartName}'.");
        }

        item.Quantity = dto.Quantity;
        item.UpdatedAt = DateTime.UtcNow;
        cart.UpdatedAt = DateTime.UtcNow;
        cartRepository.Update(cart);
        await cartRepository.SaveChangesAsync();

        var refreshed = await cartRepository.GetOrCreateByCustomerWithItemsAsync(customerUserId, trackChanges: false);
        return ApiResponse<CartDto>.SuccessResponse(MapCart(refreshed), "Cart item updated successfully.");
    }

    public async Task<ApiResponse<CartDto>> RemoveItemAsync(string customerUserId, int cartItemId)
    {
        var cart = await cartRepository.GetOrCreateByCustomerWithItemsAsync(customerUserId, trackChanges: true);
        var item = cart.CartItems.FirstOrDefault(x => x.CartItemId == cartItemId);
        if (item is null)
        {
            return ApiResponse<CartDto>.FailureResponse("Cart item not found.");
        }

        cart.CartItems.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        cartRepository.Update(cart);
        await cartRepository.SaveChangesAsync();

        var refreshed = await cartRepository.GetOrCreateByCustomerWithItemsAsync(customerUserId, trackChanges: false);
        return ApiResponse<CartDto>.SuccessResponse(MapCart(refreshed), "Cart item removed successfully.");
    }

    public async Task<ApiResponse<CartDto>> ClearAsync(string customerUserId)
    {
        var cart = await cartRepository.GetOrCreateByCustomerWithItemsAsync(customerUserId, trackChanges: true);
        if (cart.CartItems.Count > 0)
        {
            cartItemRepository.DeleteRange(cart.CartItems.ToList());
        }

        cart.UpdatedAt = DateTime.UtcNow;
        cartRepository.Update(cart);
        await cartRepository.SaveChangesAsync();

        var refreshed = await cartRepository.GetOrCreateByCustomerWithItemsAsync(customerUserId, trackChanges: false);
        return ApiResponse<CartDto>.SuccessResponse(MapCart(refreshed), "Cart cleared successfully.");
    }

    public async Task<ApiResponse<SaleCreateResponseDto>> CheckoutAsync(string customerUserId)
    {
        var cart = await cartRepository.GetOrCreateByCustomerWithItemsAsync(customerUserId, trackChanges: true);
        if (cart.CartItems.Count == 0)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Cart is empty.",
                new Dictionary<string, List<string>> { { "items", ["Cart is empty."] } });
        }

        var checkoutItems = cart.CartItems
            .Select(ci => new CreateSaleItemDto
            {
                PartId = ci.PartId,
                Quantity = ci.Quantity
            })
            .ToList();

        var saleResponse = await saleService.CreateOnlineAsync(
            new CreateOnlineSaleDto { Items = checkoutItems },
            customerUserId);

        if (!saleResponse.Success)
        {
            return saleResponse;
        }

        cartItemRepository.DeleteRange(cart.CartItems.ToList());
        cart.UpdatedAt = DateTime.UtcNow;
        cartRepository.Update(cart);
        await cartRepository.SaveChangesAsync();

        return saleResponse;
    }

    private static CartDto MapCart(Cart cart)
    {
        var items = cart.CartItems
            .OrderBy(ci => ci.CartItemId)
            .Select(ci => new CartItemDto
            {
                CartItemId = ci.CartItemId,
                PartId = ci.PartId,
                Sku = ci.Part.Sku,
                PartName = ci.Part.PartName,
                UnitPrice = ci.Part.SellingPrice,
                Quantity = ci.Quantity,
                CurrentStock = ci.Part.CurrentStock,
                IsActive = ci.Part.IsActive,
                LineTotal = ci.Part.SellingPrice * ci.Quantity
            })
            .ToList();

        var subTotal = items.Sum(x => x.LineTotal);
        var discountPercent = subTotal > LoyaltyThresholdAmount ? LoyaltyDiscountPercent : 0m;
        var discountAmount = Math.Round((subTotal * discountPercent) / 100m, 2, MidpointRounding.AwayFromZero);
        var totalAmount = subTotal - discountAmount;

        return new CartDto
        {
            CartId = cart.CartId,
            CustomerUserId = cart.CustomerApplicationUserId,
            Items = items,
            TotalUniqueItems = items.Count,
            TotalQuantity = items.Sum(x => x.Quantity),
            SubTotal = subTotal,
            LoyaltyDiscountPercent = discountPercent,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            UpdatedAt = cart.UpdatedAt
        };
    }
}
