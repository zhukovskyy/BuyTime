namespace BuyTime_Domain.Entities;

public class Feedback
{
    public Guid Id { get; set; }

    // Кому ставиться відгук 
    public Guid ExpertId { get; set; }
    public User Expert { get; set; }

    // Хто ставить відгук 
    public Guid StudentId { get; set; }
    public User Student { get; set; }

    public decimal Rating { get; set; }

    // коментар може бути null, просто рейтинг (зірочки)
    public string? Comment { get; set; }

    private DateTime _createdAt;
    public DateTime CreatedAt
    {
        get { return _createdAt; }
        set { _createdAt = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
    }
}