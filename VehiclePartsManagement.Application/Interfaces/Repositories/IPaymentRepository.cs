using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IPaymentRepository : IRepositoryBase<Payment>
{
    Task<(List<Payment> Items, int TotalItems)> GetCustomerPaymentHistoryAsync(
        string customerUserId,
        int page,
        int pageSize);
}
