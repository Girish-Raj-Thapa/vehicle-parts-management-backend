using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class ReportRepository(AppDbContext context) : IReportRepository
{
    protected AppDbContext Context { get; } = context;
}
