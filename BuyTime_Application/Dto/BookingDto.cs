namespace BuyTime_Application.Dto
{
    public class BookingDto
    {
        public string Status { get; set; } 
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserEmail { get; set; }

        public string TeacherFirstName { get; set; }
        public string TeacherLastName { get; set; }
        public string TeacherEmail { get; set; }
        public string? TeacherDescription { get; set; }
        public decimal? TeacherRating { get; set; }

        public DateTime TimeSlotStartTime { get; set; }
        public DateTime TimeSlotEndTime { get; set; }
        public bool TimeSlotIsAvailable { get; set; }
    }
}