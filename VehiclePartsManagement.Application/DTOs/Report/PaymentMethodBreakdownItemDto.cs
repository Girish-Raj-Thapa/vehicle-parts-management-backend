namespace VehiclePartsManagement.Application.DTOs.Report;

public class PaymentMethodBreakdownItemDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}