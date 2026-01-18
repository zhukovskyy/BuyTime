namespace BuyTime_Domain.Entities;

public class FavoriteExpert
{
    public Guid StudentId { get; set; }
    public User Student { get; set; }

    public Guid ExpertId { get; set; }
    public User Expert { get; set; }
}