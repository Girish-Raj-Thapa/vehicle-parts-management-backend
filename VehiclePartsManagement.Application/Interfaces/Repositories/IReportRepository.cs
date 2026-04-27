using VehiclePartsManagement.Application.DTOs.Reports;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IReportRepository
{
    Task<IEnumerable<RegularCustomerReportDto>> GetRegularCustomersAsync();
    Task<IEnumerable<TopSpenderReportDto>> GetTopSpendersAsync();
    Task<IEnumerable<PendingCreditReportDto>> GetPendingCreditsAsync();
}
