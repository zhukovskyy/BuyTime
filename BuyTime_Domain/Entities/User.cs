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

    public ICollection<LanguageSkill>? LanguageSkills { get; set; }
    public ICollection<SocialLink>? SocialLinks { get; set; }

    // user as expert
    public ICollection<Timeslot>? TimeSlots { get; set; }

    // user as student (booking history)
    public ICollection<Booking>? Bookings { get; set; }

    // Відгуки, які отримав цей юзер (якщо він Експерт)
    public ICollection<Feedback>? ReceivedFeedbacks { get; set; }

    public ICollection<Wallet> Wallets { get; set; }
}