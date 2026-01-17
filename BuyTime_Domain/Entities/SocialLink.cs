namespace BuyTime_Domain.Entities;

public class SocialLink
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public string Network { get; set; }     
    public string UrlOrHandle { get; set; } 
}