namespace VehiclePartsManagement.Application.DTOs.Report;

public class PaymentCollectionPeriodItemDto
{
    public string Period { get; set; } = string.Empty;
    public decimal CollectedAmount { get; set; }
    public int PaymentCount { get; set; }
}