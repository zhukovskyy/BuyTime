namespace BuyTime_Domain.Entities;

public class Wallet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string Network { get; set; }

    public string Address { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; }
}