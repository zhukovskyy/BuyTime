using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Notifications.Query.GetByUserId;

public class GetNotificationsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetNotificationsQuery, ErrorOr<List<NotificationDto>>>
{
    public async Task<ErrorOr<List<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notificationsResult = await unitOfWork.Notifications.GetAllAsync();

        if (notificationsResult.IsError)
            return notificationsResult.Errors;

        var notifications = notificationsResult.Value
            .Where(n => n.UserId == request.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        return notifications.Adapt<List<NotificationDto>>();
    }
}