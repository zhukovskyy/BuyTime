namespace BuyTime_Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TimeslotId { get; set; }
    public string Status { get; set; } 
    public string? Message { get; set; }
    public string? UrlOfMeeting { get; set; }
    public string? Answer { get; set; }
    private DateTime _createdAt;

    public DateTime CreatedAt
    {
        get { return _createdAt; }
        set { _createdAt = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
    }

    public User User { get; set; }
    public Timeslot TimeSlot { get; set; }
}