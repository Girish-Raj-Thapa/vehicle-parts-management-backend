using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Sales;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface ISaleService
{
    Task<ApiResponse> SendInvoiceEmailAsync(int saleId);
    Task<ApiResponse<IEnumerable<SaleSummaryDto>>> GetAllSalesAsync();
}
