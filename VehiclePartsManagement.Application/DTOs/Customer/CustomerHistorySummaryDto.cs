namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerHistorySummaryDto
{
    public int TotalPurchases { get; set; }
    public int TotalServices { get; set; }
    public int TotalVehicles { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal PendingCredit { get; set; }
    public int UpcomingAppointments { get; set; }
}
