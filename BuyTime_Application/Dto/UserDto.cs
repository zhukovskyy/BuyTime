namespace BuyTime_Application.Dto;

public class UserDto
{
    public Guid Id { get; set; }
    public bool IsTeacher { get; set; } = false;
    public string? TeacherNickname { get; set; } 
    public string TelegramChatId { get; set; } 
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? Description { get; set; }
    public decimal? Rating { get; set; }
    public string? Tags { get; set; }
    public List<TimeslotDto> Timeslots { get; set; }
    public List<FeedbackDto> Feedbacks { get; set; }
    public List<BookingDto> Bookings { get; set; }
}