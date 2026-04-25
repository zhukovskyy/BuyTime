using BuyTime_Domain.Enums;

namespace BuyTime_Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public User Student { get; set; }

    public string StudentWalletAddress { get; set; }

    public Guid TimeslotId { get; set; }
    public Timeslot TimeSlot { get; set; }

    public string Status { get; set; } 

    // повідомлення експерту від студента при створенні
    public string? MessageToExpert { get; set; }

    // повідомлення від експерта до студента при підтвердженні
    public string? ConfirmationMessage { get; set; }

    public string? MeetingLink { get; set; }
    public ICollection<MeetingAttendance> Attendances { get; set; } = new List<MeetingAttendance>();
    public string ContractAddress { get; set; }

    // звязок з скасуванням (буде null, якщо не скасовано)
    public BookingCancellation? Cancellation { get; set; }

    private DateTime _createdAt;
    public DateTime CreatedAt
    {
        get { return _createdAt; }
        set { _createdAt = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
    }
}