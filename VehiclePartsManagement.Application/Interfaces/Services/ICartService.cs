using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Cart;
using VehiclePartsManagement.Application.DTOs.Sale;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface ICartService
{
    Task<ApiResponse<CartDto>> GetMyCartAsync(string customerUserId);
    Task<ApiResponse<CartDto>> AddItemAsync(string customerUserId, AddCartItemDto dto);
    Task<ApiResponse<CartDto>> UpdateItemAsync(string customerUserId, int cartItemId, UpdateCartItemDto dto);
    Task<ApiResponse<CartDto>> RemoveItemAsync(string customerUserId, int cartItemId);
    Task<ApiResponse<CartDto>> ClearAsync(string customerUserId);
    Task<ApiResponse<SaleCreateResponseDto>> CheckoutAsync(string customerUserId);
}
