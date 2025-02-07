namespace BuyTime_Application.Dto;

public class TimeslotDto
{
    public Guid Id { get; set; }
    
    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsAvailable { get; set; }
    public Guid UserId { get; set; }
}