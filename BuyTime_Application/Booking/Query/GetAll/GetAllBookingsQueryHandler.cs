using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Booking.Query.GetAll;

public class GetAllBookingsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllBookingsQuery, ErrorOr<IEnumerable<BookingDto>>>
{
    public async Task<ErrorOr<IEnumerable<BookingDto>>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var bookings = await unitOfWork.Booking.GetAllAsync();
            
            var bookingDtos = bookings.Value.Adapt<List<BookingDto>>();

            return bookingDtos;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving bookings: {ex.Message}");
        }
    }
}