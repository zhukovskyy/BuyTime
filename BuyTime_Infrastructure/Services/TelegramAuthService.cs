using System.Security.Cryptography;
using System.Text;
using System.Web;
using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Infrastructure.Common.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace BuyTime_Infrastructure.Services;

public class TelegramAuthService(IOptions<TelegramSettings> settings) : ITelegramAuthService
{
    private readonly string _botToken = settings.Value.BotToken
        ?? throw new ArgumentNullException("Telegram:BotToken is missing in appsettings");

    public bool ValidateInitData(string initData, out TelegramUserData? userData)
    {
        userData = null;
        var parsedParams = HttpUtility.ParseQueryString(initData);
        var hash = parsedParams["hash"];

        if (string.IsNullOrEmpty(hash)) return false;

        var sortedParams = new SortedDictionary<string, string>();
        foreach (var key in parsedParams.AllKeys)
        {
            if (key != null && key != "hash")
                sortedParams[key] = parsedParams[key]!;
        }

        var dataCheckString = string.Join("\n", sortedParams.Select(kv => $"{kv.Key}={kv.Value}"));

        using var hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes("WebAppData"));
        var secretKey = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(_botToken));

        using var hmac = new HMACSHA256(secretKey);
        var computedHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
        var computedHashHex = Convert.ToHexString(computedHashBytes).ToLower();

        if (computedHashHex != hash) return false;

        var userJson = parsedParams["user"];
        if (!string.IsNullOrEmpty(userJson))
        {
            var jUser = JObject.Parse(userJson);
            var id = jUser["id"]?.ToString() ?? "";
            var username = jUser["username"]?.ToString() ?? "";
            var firstName = jUser["first_name"]?.ToString() ?? "Unknown";
            var lastName = jUser["last_name"]?.ToString() ?? "";

            userData = new TelegramUserData(id, username, firstName, lastName);
        }

        return true;
    }
}