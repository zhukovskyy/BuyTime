using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Query.GetUserByChatId;

public record GetUserByChatIdQuery(string ChatId) : IRequest<ErrorOr<BuyTime_Domain.Entities.User>>;