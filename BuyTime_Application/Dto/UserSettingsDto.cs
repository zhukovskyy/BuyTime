namespace BuyTime_Application.Dto;

public class UserSettingsDto
{
    public string Theme { get; set; }
    public string Language { get; set; }

    public bool ShowCurrencyEquivalent { get; set; }
    public string Currency { get; set; }

    public bool NotifyInTelegram { get; set; }
    public bool NotifyOnBooking { get; set; }
    public bool NotifyOnFinance { get; set; }
    public bool NotifyReminders { get; set; }
    public bool NotifyOnNewFeedback { get; set; }
}