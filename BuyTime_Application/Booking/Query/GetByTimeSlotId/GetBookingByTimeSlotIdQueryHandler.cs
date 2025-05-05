using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Booking.Query.GetByTimeSlotId;

public class GetBookingsByTimeSlotIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookingsByTimeSlotIdQuery, ErrorOr<List<BookingDto>>>
{
    public async Task<ErrorOr<List<BookingDto>>> Handle(GetBookingsByTimeSlotIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var bookings = await unitOfWork.Booking.GetBookingsByTimeSlotIdAsync(request.TimeSlotId);
            if (bookings.IsError)
            {
                return bookings.Errors;
            }
            var bookingDtos = bookings.Value.Adapt<List<BookingDto>>();
            return bookingDtos;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving bookings: {ex.Message}");
        }
    }
}