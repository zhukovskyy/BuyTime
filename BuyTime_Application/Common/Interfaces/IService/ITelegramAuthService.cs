namespace BuyTime_Application.Common.Interfaces.IService;

public record TelegramUserData(string Id, string Username, string FirstName, string LastName);

public interface ITelegramAuthService
{
    bool ValidateInitData(string initData, out TelegramUserData? userData);
}