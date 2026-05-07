using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IVehicleRepository : IRepositoryBase<Vehicle>
{
    /// <summary>
    /// Returns a vehicle by ID.
    /// </summary>
    Task<Vehicle?> GetByIdAsync(int vehicleId);

    /// <summary>
    /// Checks ownership of a vehicle by application user.
    /// </summary>
    Task<bool> IsOwnedByUserAsync(int vehicleId, string applicationUserId);
}
