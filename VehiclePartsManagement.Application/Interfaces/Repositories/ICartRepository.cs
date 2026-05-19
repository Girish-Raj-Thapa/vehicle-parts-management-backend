using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface ICartRepository : IRepositoryBase<Cart>
{
    Task<Cart> GetOrCreateByCustomerWithItemsAsync(string customerUserId, bool trackChanges);
}
