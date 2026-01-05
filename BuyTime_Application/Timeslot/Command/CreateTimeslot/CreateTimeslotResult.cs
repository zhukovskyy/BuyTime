namespace BuyTime_Application.Timeslot.CreateTimeslot;

public record CreateTimeslotResult
{
    public Guid TimeslotId { get; init; }
    public Guid ExpertId { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public bool IsAvailable { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; }
}