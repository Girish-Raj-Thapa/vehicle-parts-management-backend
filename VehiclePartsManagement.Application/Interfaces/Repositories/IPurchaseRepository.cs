using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IPurchaseRepository : IRepositoryBase<Purchase>
{
    Task<(List<Purchase> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        int? vendorId,
        DateTime? fromDate,
        DateTime? toDate);

    Task<Purchase?> GetByIdWithDetailsAsync(int purchaseId);

    Task CreateInvoiceWithStockUpdatesAsync(
        Purchase purchase,
        IEnumerable<Part> partsToUpdate,
        IEnumerable<StockMovement> stockMovements);
}
