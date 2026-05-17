using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

/// <summary>
/// Repository contract for stock movement persistence operations.
/// </summary>
public interface IStockMovementRepository : IRepositoryBase<StockMovement>
{
}
