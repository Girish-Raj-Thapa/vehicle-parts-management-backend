using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IPartRepository : IRepositoryBase<Part>
{
    Task<Part?> GetByIdAsync(int partId);
    Task<Part?> GetByIdWithCategoryAsync(int partId);
    Task<List<Part>> GetAllWithCategoryAsync();
}
