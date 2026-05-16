namespace BuyTime_Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }

    public string Title { get; set; }
    public string Message { get; set; }

    public string Type { get; set; }
    public string? ActionData { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }
}