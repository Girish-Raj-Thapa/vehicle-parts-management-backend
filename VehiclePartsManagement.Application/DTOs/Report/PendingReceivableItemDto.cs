namespace VehiclePartsManagement.Application.DTOs.Report;

public class PendingReceivableItemDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal BalanceDue { get; set; }
}