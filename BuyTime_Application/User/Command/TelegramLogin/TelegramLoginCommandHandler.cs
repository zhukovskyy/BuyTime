using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Command.TelegramLogin;

public class TelegramLoginCommandHandler(
    ITelegramAuthService telegramAuthService,
    IUnitOfWork unitOfWork,
    IJwtProvider jwtProvider)
    : IRequestHandler<TelegramLoginCommand, ErrorOr<TelegramLoginResult>>
{
    public async Task<ErrorOr<TelegramLoginResult>> Handle(TelegramLoginCommand request, CancellationToken cancellationToken)
    {
        if (!telegramAuthService.ValidateInitData(request.InitData, out var telegramUser) || telegramUser == null)
        {
            return Error.Unauthorized("Auth.InvalidSignature", "Недійсний підпис Telegram.");
        }

        var userResult = await unitOfWork.User.GetUserByChatIdAsync(telegramUser.Id);

        if (userResult.IsError)
        {
            return Error.NotFound("User.NotRegistered", "Користувач не зареєстрований. Необхідна реєстрація.");
        }

        var user = userResult.Value;

        var settings = await unitOfWork.UserSettings.GetByUserIdAsync(user.Id);
        if (settings != null && settings.Timezone != request.Timezone)
        {
            settings.Timezone = request.Timezone;
            await unitOfWork.UserSettings.UpdateAsync(settings);
            await unitOfWork.CommitAsync();
        }

        var token = jwtProvider.GenerateToken(user);
        return new TelegramLoginResult(token, user.IsExpert);
    }
}