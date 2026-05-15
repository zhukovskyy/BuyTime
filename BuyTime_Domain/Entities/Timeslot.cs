namespace BuyTime_Domain.Entities;

public class Timeslot
{
    public Guid Id { get; set; }

    public Guid ExpertId { get; set; }
    public User Expert { get; set; }

    private DateTime _startTime;
    public DateTime StartTime
    {
        get { return _startTime; }
        set { _startTime = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
    }

    private DateTime _endTime;
    public DateTime EndTime
    {
        get { return _endTime; }
        set { _endTime = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
    }

    public decimal Price { get; set; }
    public string Currency { get; set; } = "TON";
    public bool IsAvailable { get; set; }

    public string? ExpertWalletAddress { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}