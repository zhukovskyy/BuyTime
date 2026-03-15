using ErrorOr;
using MediatR;

namespace BuyTime_Application.Common.Interfaces.IService;

public interface IBookingService
{
    Task<ErrorOr<Unit>> ConfirmBookingAsync(Guid bookingId, string confirmationMessage, string meetingLink);

    Task<Guid> CreateBookingAsync(Guid studentId, Guid timeslotId, string messageToExpert, string contractAddress, string studentWalletAddress);

    Task<ErrorOr<Unit>> CancelBookingAsync(Guid bookingId, string reason, Guid triggeredByUserId);
}