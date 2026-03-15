using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Command.ResolveByArbiter;

public record ResolveBookingByArbiterCommand(
    Guid BookingId,
    bool IsExpertPresent,
    bool IsStudentPresent
) : IRequest<ErrorOr<Unit>>;