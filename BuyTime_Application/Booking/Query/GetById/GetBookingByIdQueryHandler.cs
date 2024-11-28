using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Application.Timeslot.Query.GetById;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Booking.Query.GetById;

public class GetBookingByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookingByIdQuery, ErrorOr<List<BookingDto>>>
{
    public async Task<ErrorOr<List<BookingDto>>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        try 
        {
            var booking = await unitOfWork.Booking.GetByIdAsync(request.Id);
            if (booking == null) 
            {
                return Error.Failure("Booking not found."); 
            }
            var bookingDto = booking.Adapt<BookingDto>();
            return new List<BookingDto> { bookingDto }; 
        } 
        catch (Exception ex)
        {
            return Error.Failure("Error while retrieving booking."); 
        }
    }
}