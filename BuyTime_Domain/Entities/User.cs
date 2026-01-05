namespace BuyTime_Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string? Email { get; set; }

    public string TelegramChatId { get; set; }

    public bool IsExpert { get; set; } = false;
    public string? ExpertNickname { get; set; }

    public string? Description { get; set; }
    public decimal? Rating { get; set; }
    public string? Tags { get; set; }

    // user as expert
    public ICollection<Timeslot>? TimeSlots { get; set; }

    // user as student
    public ICollection<Booking>? Bookings { get; set; }
    public ICollection<Feedback>? Feedbacks { get; set; }
    public ICollection<Wallet> Wallets { get; set; }

}