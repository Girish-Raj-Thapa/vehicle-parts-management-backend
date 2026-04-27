using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IPartRepository : IRepositoryBase<Part>
{
    Task<IEnumerable<Part>> GetLowStockPartsAsync();
}
