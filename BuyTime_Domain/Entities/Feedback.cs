namespace BuyTime_Domain.Entities;

public class Feedback
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Rating { get; set; }
    public string? Comment { get; set; }
    private DateTime _createdAt;
    public DateTime CreatedAt
    {
        get { return _createdAt; }
        set {_createdAt = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
    }
    public User User { get; set; }
}