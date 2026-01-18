using System.Text.Json.Serialization; 

namespace BuyTime_Domain.Entities;

public class SocialMediaPlatform
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? LogoUrl { get; set; }

    [JsonIgnore]
    public ICollection<ExpertSocialLink> ExpertLinks { get; set; }
}