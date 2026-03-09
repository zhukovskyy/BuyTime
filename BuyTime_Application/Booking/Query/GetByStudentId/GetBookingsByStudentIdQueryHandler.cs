using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Booking.Query.GetByStudentId;

public class GetBookingsByStudentIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookingsByStudentIdQuery, ErrorOr<List<StudentBookingSummaryDto>>>
{
    public async Task<ErrorOr<List<StudentBookingSummaryDto>>> Handle(GetBookingsByStudentIdQuery request, CancellationToken cancellationToken)
    {
        var bookings = await unitOfWork.Booking.GetBookingsByStudentIdAsync(request.StudentId);

        if (bookings.IsError)
            return bookings.Errors;

        return bookings.Value.Adapt<List<StudentBookingSummaryDto>>();
    }
}