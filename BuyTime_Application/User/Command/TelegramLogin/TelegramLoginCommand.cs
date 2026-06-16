using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Command.TelegramLogin;

public record TelegramLoginResult(string Token, bool IsExpert);

public record TelegramLoginCommand(string InitData) : IRequest<ErrorOr<TelegramLoginResult>>;