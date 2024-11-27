namespace BuyTime_Domain.Entities;

public class Timeslot
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }

    public Teacher Teacher { get; set; }
}