namespace BuyTime_Domain.Entities;

public class Feedback
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid UserId { get; set; }
    public decimal Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public Teacher Teacher { get; set; }
    public User User { get; set; }
}