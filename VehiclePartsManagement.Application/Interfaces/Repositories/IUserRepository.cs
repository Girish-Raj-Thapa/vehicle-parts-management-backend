using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IUserRepository : IRepositoryBase<ApplicationUser>
{
    Task<ApplicationUser?> GetCustomerWithVehiclesAsync(string customerId);
}
