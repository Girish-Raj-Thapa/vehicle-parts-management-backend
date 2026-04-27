using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Reports;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IReportService
{
    Task<ApiResponse<IEnumerable<RegularCustomerReportDto>>> GetRegularCustomersAsync();
    Task<ApiResponse<IEnumerable<TopSpenderReportDto>>> GetTopSpendersAsync();
    Task<ApiResponse<IEnumerable<PendingCreditReportDto>>> GetPendingCreditsAsync();
}
