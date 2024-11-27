namespace BuyTime_Domain.Entities;

public class Teacher : User
{
    public string? Description { get; set; }
    public decimal? Rating { get; set; }
    public string? Tags { get; set; } 
    public ICollection<Timeslot> TimeSlots { get; set; }
    public ICollection<Booking> Bookings { get; set; }
}