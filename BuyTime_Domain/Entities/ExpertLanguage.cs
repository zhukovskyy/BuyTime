namespace BuyTime_Domain.Entities;

public class ExpertLanguage
{
    public Guid ExpertId { get; set; }
    public User Expert { get; set; }

    public Guid LanguageId { get; set; }
    public Language Language { get; set; }

    public string Level { get; set; }
}