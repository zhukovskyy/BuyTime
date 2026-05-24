namespace BuyTime_Application.Dto;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public bool IsExpert { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? TelegramChatId { get; set; }
    public string? DiscordId { get; set; }
    public string? Email { get; set; }

    public string? ExpertNickname { get; set; }
    public string? Description { get; set; }
    public decimal? Rating { get; set; }

    public double TotalHoursConducted { get; set; }
    public int HappyStudentsCount { get; set; }
    public int ReviewCount { get; set; }

    public List<LanguageSkillDto> LanguageSkills { get; set; } = new();
    public List<SocialLinkDto> SocialLinks { get; set; } = new();
    public List<SpecializationDto> Specializations { get; set; } = new();
}