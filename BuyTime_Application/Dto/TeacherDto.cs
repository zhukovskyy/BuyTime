namespace BuyTime_Application.Dto;

public class TeacherDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? Description { get; set; }
    public decimal? Rating { get; set; }
    public string? Tags { get; set; }
    public string Role { get; set; }
    public List<TimeslotDto> Timeslots { get; set; }
}