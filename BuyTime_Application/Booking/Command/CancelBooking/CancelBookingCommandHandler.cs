using BuyTime_Application.Booking.Command.ConfirmBooking;
using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Command.CancelBooking;

public class CancelBookingCommandHandler(IUnitOfWork unitOfWork, IBookingService bookingService)
    : IRequestHandler<CancelBookingCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        return await bookingService.CancelBookingAsync(request.BookingId, request.CancellationMessage);
    }
}