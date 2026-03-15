namespace BuyTime_Application.Dto
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid TimeslotId { get; set; }

        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? MeetingLink { get; set; }

        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string? StudentEmail { get; set; }
        public string? MessageToExpert { get; set; }

        public string ExpertFirstName { get; set; }
        public string ExpertLastName { get; set; }
        public string? ConfirmationMessage { get; set; }
        public string? ExpertEmail { get; set; }

        public string? ExpertDescription { get; set; }
        public decimal? ExpertRating { get; set; }

        public DateTime TimeSlotStartTime { get; set; }
        public DateTime TimeSlotEndTime { get; set; }
        public bool TimeSlotIsAvailable { get; set; }
        public decimal TimeSlotPrice { get; set; }
        public string TimeSlotCurrency { get; set; }

        public string ContractAddress { get; set; }

        public string? CancellationReason { get; set; }
        public string? CancelledByRole { get; set; }
    }
}