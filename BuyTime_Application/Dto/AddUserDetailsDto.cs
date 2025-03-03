namespace BuyTime_Application.Dto;

public class AddUserDetailsDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string TelegramChatId { get; set; }
    public bool IsTeacher { get; set; } = false;
    public string? TeacherNickname { get; set; } 
    public string? Description { get; set; } 
    public decimal? Rating { get; set; } 
    public string? Tags { get; set; } 
}