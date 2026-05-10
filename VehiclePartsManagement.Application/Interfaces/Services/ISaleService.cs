using VehiclePartsManagement.Application.Common;
<<<<<<< HEAD
using VehiclePartsManagement.Application.DTOs.Sales;
=======
using VehiclePartsManagement.Application.DTOs.Sale;
>>>>>>> prashish

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface ISaleService
{
    Task<ApiResponse> SendInvoiceEmailAsync(int saleId);
    Task<ApiResponse<IEnumerable<SaleSummaryDto>>> GetAllSalesAsync();
    Task<ApiResponse<SaleCreateResponseDto>> CreateAsync(CreateSaleDto dto, string processedByApplicationUserId);
    Task<PagedResponse<List<SaleListItemDto>>> GetPagedAsync(SaleListQueryDto query, string? customerScopeUserId);
    Task<ApiResponse<SaleDetailDto>> GetByIdAsync(int saleId, string? customerScopeUserId);
    Task<ApiResponse<SaleInvoiceDto>> GetInvoiceAsync(int saleId, string? customerScopeUserId);
}
