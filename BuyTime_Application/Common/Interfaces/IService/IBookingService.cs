using ErrorOr;
using MediatR;

namespace BuyTime_Application.Common.Interfaces.IService;

public interface IBookingService
{
    Task<ErrorOr<Unit>> ConfirmBookingAsync(Guid bookingId, string confirmationMessage, string contactLink);
    Task<Guid> CreateBookingAsync(Guid userId, Guid teacherId, Guid timeslotId, string status, string message);
}