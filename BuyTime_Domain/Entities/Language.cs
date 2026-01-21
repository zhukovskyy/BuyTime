namespace BuyTime_Domain.Entities;

public class Language
{
    public Guid Id { get; set; }
    public string? Code { get; set; }

    public ICollection<ExpertLanguage> ExpertLanguages { get; set; }
}