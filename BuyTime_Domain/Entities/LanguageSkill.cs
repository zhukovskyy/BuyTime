namespace BuyTime_Domain.Entities;

public class LanguageSkill
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public string LanguageName { get; set; } 
    public string Level { get; set; }        
}