using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IServiceAppointmentRepository : IRepositoryBase<ServiceAppointment>
{
    /// <summary>
    /// Returns paged appointments with optional owner and status filters.
    /// </summary>
    Task<(List<ServiceAppointment> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        string? applicationUserId,
        ServiceAppointmentStatus? status,
        DateOnly? appointmentDate = null);

    /// <summary>
    /// Returns a single appointment with user and vehicle details.
    /// </summary>
    Task<ServiceAppointment?> GetByIdWithDetailsAsync(int appointmentId);
    /// <summary>
    /// Returns a single appointment for a specific user scope.
    /// </summary>
    Task<ServiceAppointment?> GetByIdForUserAsync(int appointmentId, string applicationUserId);

    /// <summary>
    /// Returns a tracked appointment entity for update operations.
    /// </summary>
    Task<ServiceAppointment?> GetByIdForUpdateAsync(int appointmentId);
    /// <summary>
    /// Checks whether the vehicle belongs to the provided user.
    /// </summary>
    Task<bool> IsVehicleOwnedByUserAsync(int vehicleId, string applicationUserId);

    /// <summary>
    /// Returns active appointment count for a specific date and time slot.
    /// </summary>
    Task<int> CountActiveByDateTimeAsync(DateTime appointmentDate, string appointmentTime);
}
