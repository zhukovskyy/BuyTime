namespace BuyTime_Application.Dto;

public class TimeslotDto
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; }

    public Guid ExpertId { get; set; }

    public string? ExpertWalletAddress { get; set; }
}