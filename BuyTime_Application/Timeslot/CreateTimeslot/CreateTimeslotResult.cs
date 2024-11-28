namespace BuyTime_Application.Timeslot.CreateTimeslot;

public record CreateTimeslotResult
{
    public Guid TimeslotId { get; init; }
    public Guid TeacherId { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public bool IsAvailable { get; init; }
}