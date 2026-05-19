using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Sales;
using VehiclePartsManagement.Application.DTOs.Sale;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface ISaleService
{
    Task<ApiResponse> SendInvoiceEmailAsync(int saleId);
    Task<ApiResponse<IEnumerable<SaleSummaryDto>>> GetAllSalesAsync();
    Task<ApiResponse<SaleCreateResponseDto>> CreateAsync(CreateSaleDto dto, string processedByApplicationUserId);
    Task<ApiResponse<SaleCreateResponseDto>> CreateOnlineAsync(CreateOnlineSaleDto dto, string customerUserId);
    Task<PagedResponse<List<SaleListItemDto>>> GetPagedAsync(SaleListQueryDto query, string? customerScopeUserId);
    Task<ApiResponse<SaleDetailDto>> GetByIdAsync(int saleId, string? customerScopeUserId);
    Task<ApiResponse<SaleInvoiceDto>> GetInvoiceAsync(int saleId, string? customerScopeUserId);
    Task<ApiResponse<SaleDetailDto>> GetMyOnlineOrderByIdAsync(int saleId, string customerUserId);
    Task<ApiResponse<SaleInvoiceDto>> GetMyOnlineOrderInvoiceAsync(int saleId, string customerUserId);
    Task<ApiResponse<List<RecentPurchasedPartDto>>> GetRecentPurchasedPartsAsync(string customerUserId, int limit);
}
