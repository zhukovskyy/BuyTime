namespace BuyTime_Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Email { get; set; }
    public string TelegramChatId { get; set; }
    public string? DiscordId { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsExpert { get; set; } = false;
    public string? ExpertNickname { get; set; }
    public string? Description { get; set; }
    public decimal? Rating { get; set; }

    public ICollection<ExpertLanguage> ExpertLanguages { get; set; }
    public ICollection<ExpertSocialLink>? SocialLinks { get; set; }
    public ICollection<Specialization> Specializations { get; set; } = new List<Specialization>();
    // user as expert
    public ICollection<Timeslot>? TimeSlots { get; set; }

    // user as student (booking history)
    public ICollection<Booking>? Bookings { get; set; }

    // Відгуки, які отримав цей юзер (якщо він Експерт)
    public ICollection<Feedback>? ReceivedFeedbacks { get; set; }
    public ICollection<FavoriteExpert>? FavoriteExperts { get; set; }
    public ICollection<Wallet> Wallets { get; set; }

    public UserSettings? Settings { get; set; }

}