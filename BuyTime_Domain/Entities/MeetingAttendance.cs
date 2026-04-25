using BuyTime_Domain.Enums;

namespace BuyTime_Domain.Entities;

public class MeetingAttendance
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; }
    public string ExternalMeetingId { get; set; } = string.Empty; // ID кімнати
    public MeetingPlatform Platform { get; set; }
    public ulong ExternalUserId { get; set; } // Наприклад, DiscordId користувача
    public Guid? SystemUserId { get; set; } // userID з нашої бази
    public DateTime FirstJoinedAt { get; set; }
}