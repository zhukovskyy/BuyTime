namespace BuyTime_Application.Dto;

public record TelegramAuthRequest(string InitData, string Timezone = "UTC");