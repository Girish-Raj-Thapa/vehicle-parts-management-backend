using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Reports;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Application.Services;

public class ReportService(IReportRepository reportRepository) : IReportService
{
    public async Task<ApiResponse<IEnumerable<RegularCustomerReportDto>>> GetRegularCustomersAsync()
    {
        var data = await reportRepository.GetRegularCustomersAsync();
        return ApiResponse<IEnumerable<RegularCustomerReportDto>>.SuccessResponse(data, "Regular customers report retrieved successfully");
    }

    public async Task<ApiResponse<IEnumerable<TopSpenderReportDto>>> GetTopSpendersAsync()
    {
        var data = await reportRepository.GetTopSpendersAsync();
        return ApiResponse<IEnumerable<TopSpenderReportDto>>.SuccessResponse(data, "Top spenders report retrieved successfully");
    }

    public async Task<ApiResponse<IEnumerable<PendingCreditReportDto>>> GetPendingCreditsAsync()
    {
        var data = await reportRepository.GetPendingCreditsAsync();
        return ApiResponse<IEnumerable<PendingCreditReportDto>>.SuccessResponse(data, "Pending credits report retrieved successfully");
    }
}
