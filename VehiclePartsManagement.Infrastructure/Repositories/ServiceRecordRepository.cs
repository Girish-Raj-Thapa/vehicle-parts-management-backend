using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class ServiceRecordRepository(AppDbContext context)
    : RepositoryBase<ServiceRecord>(context), IServiceRecordRepository
{
    public async Task<(List<ServiceRecord> Items, int TotalItems)> GetCustomerServiceRecordsAsync(
        string customerUserId,
        int page,
        int pageSize,
        int? vehicleId)
    {
        var query = Context.ServiceRecords
            .AsNoTracking()
            .Include(sr => sr.Vehicle)
            .ThenInclude(v => v.Model)
            .ThenInclude(m => m.Brand)
            .Include(sr => sr.ServiceAppointment)
            .Where(sr => sr.Vehicle.ApplicationUserId == customerUserId);

        if (vehicleId.HasValue)
        {
            query = query.Where(sr => sr.VehicleId == vehicleId.Value);
        }

        var totalItems = await query.CountAsync();
        if (totalItems == 0)
        {
            return ([], 0);
        }

        var items = await query
            .OrderByDescending(sr => sr.ServiceDate)
            .ThenByDescending(sr => sr.ServiceRecordId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }


    public Task<ServiceRecord?> GetCustomerServiceRecordDetailAsync(string customerUserId, int serviceRecordId) =>
        Context.ServiceRecords
            .AsNoTracking()
            .Include(sr => sr.Vehicle)
            .ThenInclude(v => v.Model)
            .ThenInclude(m => m.Brand)
            .Include(sr => sr.ServiceAppointment)
            .FirstOrDefaultAsync(sr =>
                sr.ServiceRecordId == serviceRecordId &&
                sr.Vehicle.ApplicationUserId == customerUserId);
}
