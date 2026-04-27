using VehiclePartsManagement.Application.Common;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface ISaleService
{
    Task<ApiResponse> SendInvoiceEmailAsync(int saleId);
}
