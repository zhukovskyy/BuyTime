namespace BuyTime_Application.Dto;

public class StudentDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public List<FeedbackDto> Feedbacks { get; set; }
    public List<BookingDto> Bookings { get; set; }
}