using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IServiceRecordRepository : IRepositoryBase<ServiceRecord>
{
    Task<(List<ServiceRecord> Items, int TotalItems)> GetCustomerServiceRecordsAsync(
        string customerUserId,
        int page,
        int pageSize,
        int? vehicleId);

    Task<ServiceRecord?> GetCustomerServiceRecordDetailAsync(string customerUserId, int serviceRecordId);
}
