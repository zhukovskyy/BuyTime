namespace BuyTime_Application.Dto;

public class StudentBookingSummaryDto
{
    public Guid Id { get; set; } // BookingId
    public Guid TimeslotId { get; set; }

    public string Status { get; set; }
    public string ExpertFirstName { get; set; }
    public string ExpertLastName { get; set; }
    public DateTime TimeSlotStartTime { get; set; }
    public DateTime TimeSlotEndTime { get; set; }
    public decimal TimeSlotPrice { get; set; }
    public string TimeSlotCurrency { get; set; }

    public string? CancellationReason { get; set; }
    public string? CancelledByRole { get; set; }

    public string? MessageToExpert { get; set; }
    public string? ConfirmationMessage { get; set; }
    public string? MeetingLink { get; set; }
}