namespace BuyTime_Application.Dto;

public class FeedbackDto
{
    public decimal Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}