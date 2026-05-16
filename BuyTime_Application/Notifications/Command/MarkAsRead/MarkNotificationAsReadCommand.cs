using ErrorOr;
using MediatR;

namespace BuyTime_Application.Notifications.Command.MarkAsRead;

public record MarkNotificationAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<ErrorOr<Unit>>;