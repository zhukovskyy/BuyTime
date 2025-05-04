using ErrorOr;
using MediatR;

namespace BuyTime_Application.Common.Interfaces.IService;

public interface IBookingService
{
    Task<ErrorOr<Unit>> ConfirmBookingAsync(Guid bookingId, string confirmationMessage, string contactLink);
    Task<Guid> CreateBookingAsync(Guid userId, Guid timeslotId, string message, string status);
    Task<ErrorOr<Unit>> CancelBookingAsync(Guid bookingId, string cancellationMessage);
}