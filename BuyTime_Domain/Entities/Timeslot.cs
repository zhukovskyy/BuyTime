namespace BuyTime_Domain.Entities;

public class Timeslot
{
    public Guid Id { get; set; }

    public Guid ExpertId { get; set; }
    public User Expert { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public decimal Price { get; set; }
    public string Currency { get; set; } = "TON";
    public bool IsAvailable { get; set; }

    public string? ExpertWalletAddress { get; set; }
}