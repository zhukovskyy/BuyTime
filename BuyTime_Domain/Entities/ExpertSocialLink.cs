namespace BuyTime_Domain.Entities;

public class ExpertSocialLink
{
    public Guid Id { get; set; }

    public Guid ExpertId { get; set; } 
    public User Expert { get; set; }

    public Guid PlatformId { get; set; }
    public SocialMediaPlatform Platform { get; set; }

    public string UrlOrHandle { get; set; }
}