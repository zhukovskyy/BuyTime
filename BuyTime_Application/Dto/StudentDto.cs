namespace BuyTime_Application.Dto;

public class StudentDto
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public List<FeedbackDto> Feedbacks { get; set; }
}