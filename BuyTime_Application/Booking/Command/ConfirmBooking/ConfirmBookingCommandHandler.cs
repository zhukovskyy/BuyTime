using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using MediatR;
using ErrorOr;

namespace BuyTime_Application.Booking.Command.ConfirmBooking;

public class ConfirmBookingCommandHandler(IUnitOfWork unitOfWork, IBookingService bookingService)
    : IRequestHandler<ConfirmBookingCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        return await bookingService.ConfirmBookingAsync(request.BookingId, request.ConfirmationMessage, request.ContactLink);
    }
}