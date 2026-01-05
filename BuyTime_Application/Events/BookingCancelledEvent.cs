using MediatR;

namespace BuyTime_Application.Events;

public record BookingCancelledEvent(Guid BookingId, string CancellationMessage, Guid triggeredByUserId) : INotification;