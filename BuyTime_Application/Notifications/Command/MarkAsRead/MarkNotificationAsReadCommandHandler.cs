using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Notifications.Command.MarkAsRead;

public class MarkNotificationAsReadCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<MarkNotificationAsReadCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await unitOfWork.Notifications.GetByIdAsync(request.NotificationId);

        if (notification == null)
            return Error.NotFound("Notification.NotFound", "Сповіщення не знайдено.");

        if (notification.UserId != request.UserId)
            return Error.Validation("AccessDenied", "Немає доступу до цього сповіщення.");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await unitOfWork.Notifications.UpdateAsync(notification);
            await unitOfWork.CommitAsync();
        }

        return Unit.Value;
    }
}