using VehiclePartsManagement.Domain.Enums;
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
    /// <summary>
    /// Returns paged sales with optional customer and payment status filters.
    /// </summary>
    Task<(List<Sale> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        string? customerApplicationUserId,
        PaymentStatus? paymentStatus);

    /// <summary>
    /// Returns one sale with full detail used by sales detail and invoice views.
    /// </summary>
    Task<Sale?> GetByIdWithDetailsAsync(int saleId);

    /// <summary>
    /// Returns one sale with full detail constrained to a specific customer scope.
    /// </summary>
    Task<Sale?> GetByIdForCustomerAsync(int saleId, string customerApplicationUserId);
}
