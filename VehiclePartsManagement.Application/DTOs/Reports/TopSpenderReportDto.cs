namespace VehiclePartsManagement.Application.DTOs.Reports;

public class TopSpenderReportDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
}
