using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface ISaleRepository : IRepositoryBase<Sale>
{
    Task<Sale?> GetSaleWithDetailsAsync(int saleId);
    Task<IEnumerable<Sale>> GetOverdueSalesAsync();
    Task<IEnumerable<Sale>> GetAllSalesAsync();
}
