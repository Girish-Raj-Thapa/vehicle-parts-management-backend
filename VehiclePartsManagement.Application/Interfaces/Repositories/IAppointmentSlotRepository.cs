using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

/// <summary>
/// Appointment-slot specific data-access contract.
/// </summary>
public interface IAppointmentSlotRepository : IRepositoryBase<AppointmentSlot>
{
    /// <summary>
    /// Returns appointment slot query with optional tracking.
    /// </summary>
    IQueryable<AppointmentSlot> GetSlotQuery(bool trackChanges = false);

    /// <summary>
    /// Checks duplicate date/time combination optionally excluding one slot id.
    /// </summary>
    Task<bool> ExistsByDateAndTimeAsync(DateTime slotDate, string slotTime, int? excludeSlotId = null);
}
