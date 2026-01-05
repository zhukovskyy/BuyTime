using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Booking.Command.CancelBooking; 
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Command.CancelBooking;

public class CancelBookingCommandHandler(IBookingService bookingService)
    : IRequestHandler<CancelBookingCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        return await bookingService.CancelBookingAsync(
            request.BookingId,
            request.CancellationMessage,
            request.TriggeredByUserId   
        );
    }
}