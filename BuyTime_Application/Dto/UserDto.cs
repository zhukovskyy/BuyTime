namespace BuyTime_Application.Dto;

public class UserDto
{
    public Guid Id { get; set; }

    public bool IsExpert { get; set; } = false;
    public string? ExpertNickname { get; set; }
    public string TelegramChatId { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string? Email { get; set; }

    public string? Description { get; set; }
    public List<LanguageSkillDto> LanguageSkills { get; set; }
    public List<SocialLinkDto> SocialLinks { get; set; }
    public decimal? Rating { get; set; }
    public string? AvatarUrl { get; set; }
    public List<TimeslotDto> TimeSlots { get; set; }
    public List<FeedbackDto> Feedbacks { get; set; }
    public List<BookingDto> Bookings { get; set; }
}