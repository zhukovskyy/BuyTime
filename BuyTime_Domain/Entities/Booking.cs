namespace BuyTime_Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid TimeslotId { get; set; }
    public string Status { get; set; } 
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
    public Teacher Teacher { get; set; }
    public Timeslot TimeSlot { get; set; }
}