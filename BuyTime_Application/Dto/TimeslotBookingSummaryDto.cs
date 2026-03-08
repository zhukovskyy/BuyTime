namespace BuyTime_Application.Dto;

public class TimeslotBookingSummaryDto
{
    public Guid Id { get; set; } // ID самого бронювання (BookingId)
    public Guid StudentId { get; set; }
    public string StudentFirstName { get; set; }
    public string StudentLastName { get; set; }

    public string Status { get; set; } // pending, confirmed, completed

    public string? MessageToExpert { get; set; }
    public string? ConfirmationMessage { get; set; }
    public string? MeetingLink { get; set; }
}