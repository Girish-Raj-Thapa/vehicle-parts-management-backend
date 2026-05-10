using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface ISaleRepository : IRepositoryBase<Sale>
{
    Task<(List<Sale> Items, int TotalItems)> GetCustomerPurchaseHistoryAsync(
        string customerUserId,
        int page,
        int pageSize,
        PaymentStatus? paymentStatus,
        DateTime? fromDate,
        DateTime? toDate);

    Task<Sale?> GetCustomerPurchaseDetailAsync(string customerUserId, int saleId);
    Task<Sale?> GetSaleWithDetailsAsync(int saleId);
    Task<IEnumerable<Sale>> GetOverdueSalesAsync();
    Task<IEnumerable<Sale>> GetAllSalesAsync();
}
