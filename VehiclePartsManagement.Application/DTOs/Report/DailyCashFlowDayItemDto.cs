namespace VehiclePartsManagement.Application.DTOs.Report;

public class DailyCashFlowDayItemDto
{
    public DateTime DateUtc { get; set; }
    public decimal Received { get; set; }
    public decimal Spent { get; set; }
    public decimal NetFlow { get; set; }
}