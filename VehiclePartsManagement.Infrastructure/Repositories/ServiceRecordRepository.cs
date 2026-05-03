using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class ServiceRecordRepository(AppDbContext context)
    : RepositoryBase<ServiceRecord>(context), IServiceRecordRepository
{
    public Task<List<ServiceRecord>> GetCustomerServiceRecordsAsync(string customerId) =>
        Context.ServiceRecords
            .Include(record => record.Vehicle)
            .Where(record => record.Vehicle.ApplicationUserId == customerId)
            .OrderByDescending(record => record.ServiceDate)
            .AsNoTracking()
            .ToListAsync();
}
