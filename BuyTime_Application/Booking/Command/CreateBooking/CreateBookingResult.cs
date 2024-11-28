namespace BuyTime_Application.Booking.Command.CreateBooking;

public class CreateBookingResult
{
    public Guid BookingId { get; set; }
    public CreateBookingResult(Guid bookingId)
    {
        BookingId = bookingId;
    }

    public CreateBookingResult()
    {
    }
}