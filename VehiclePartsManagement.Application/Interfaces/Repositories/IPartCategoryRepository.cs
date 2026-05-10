using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IPartCategoryRepository : IRepositoryBase<PartCategory>
{
    Task<PartCategory?> GetByIdAsync(int categoryId);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<List<PartCategory>> GetAllOrderedAsync();
    Task<bool> HasLinkedPartsAsync(int categoryId);
}
