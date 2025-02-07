namespace BuyTime_Domain.Entities;

public class Timeslot
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsAvailable { get; set; }
    public User User { get; set; }
}