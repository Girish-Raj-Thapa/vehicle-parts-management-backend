using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

/// <summary>
/// Default repository for stock movement persistence.
/// </summary>
public class StockMovementRepository(AppDbContext context)
    : RepositoryBase<StockMovement>(context), IStockMovementRepository
{
}
