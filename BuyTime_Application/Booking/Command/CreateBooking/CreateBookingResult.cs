using BuyTime_Application.Dto;

namespace BuyTime_Application.Booking.Command.CreateBooking;

public class CreateBookingResult
{
    public Guid BookingId { get; set; }
    public TonConnectPayloadDto TonPayload { get; set; }
    public CreateBookingResult(Guid bookingId)
    {
        BookingId = bookingId;
    }

    public CreateBookingResult()
    {
    }
}