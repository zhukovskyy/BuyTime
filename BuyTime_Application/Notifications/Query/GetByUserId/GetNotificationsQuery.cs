using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Notifications.Query.GetByUserId;

public record GetNotificationsQuery(Guid UserId) : IRequest<ErrorOr<List<NotificationDto>>>;