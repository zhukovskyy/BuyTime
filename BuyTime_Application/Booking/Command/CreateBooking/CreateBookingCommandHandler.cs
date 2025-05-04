using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using MediatR;
using ErrorOr;

namespace BuyTime_Application.Booking.Command.CreateBooking;

public class CreateBookingCommandHandler(IUnitOfWork unitOfWork, IBookingService bookingService)
    : IRequestHandler<CreateBookingCommand, ErrorOr<CreateBookingResult>>
{
    public async Task<ErrorOr<CreateBookingResult>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var bookingId = await bookingService.CreateBookingAsync(
                userId: request.UserId,
                timeslotId: request.TimeslotId,
                message: request.Message,
                status: request.Status);

            return new CreateBookingResult
            {
                BookingId = bookingId
            };
        }
        catch (InvalidOperationException ex)
        {
            return Error.Validation("BookingCreationError", ex.Message);
        }
    }
}