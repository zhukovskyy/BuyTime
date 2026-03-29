using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Command.ResolveByStudent;

public record ResolveByStudentCommand(
    Guid BookingId,
    Guid StudentId,
    bool IsSuccessful
) : IRequest<ErrorOr<ResolveByStudentResult>>;

public record ResolveByStudentResult(bool Success, string Message);