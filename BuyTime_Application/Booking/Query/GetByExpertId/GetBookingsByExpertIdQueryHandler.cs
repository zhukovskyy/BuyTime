using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Booking.Query.GetByExpertId;

public class GetBookingsByExpertIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookingsByExpertIdQuery, ErrorOr<List<BookingDto>>>
{
    public async Task<ErrorOr<List<BookingDto>>> Handle(GetBookingsByExpertIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await unitOfWork.Booking.GetBookingsByExpertIdAsync(request.ExpertId);

            if (result.IsError)
                return result.Errors;

            var bookingDtos = result.Value.Adapt<List<BookingDto>>();

            return bookingDtos;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }
}