using MediatR;

namespace BuyTime_Application.Events;

public record BookingCreatedEvent(Guid BookingId) : INotification;