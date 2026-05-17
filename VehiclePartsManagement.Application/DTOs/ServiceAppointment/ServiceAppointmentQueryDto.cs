using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.ServiceAppointment;

/// <summary>
/// Query contract for appointment listing endpoint.
/// </summary>
public class ServiceAppointmentQueryDto
{
    public bool Mine { get; set; } = false;

    [MaxLength(50)]
    public string? Status { get; set; }

    public DateOnly? Date { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
