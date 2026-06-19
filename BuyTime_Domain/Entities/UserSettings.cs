using System.ComponentModel.DataAnnotations;

namespace BuyTime_Domain.Entities;

public class UserSettings
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    [MaxLength(20)]
    public string Theme { get; set; } = "Light"; // dark

    [MaxLength(10)]
    public string Language { get; set; } = "uk"; // en

    [MaxLength(50)]
    public string Timezone { get; set; } = "UTC";

    public bool ShowCurrencyEquivalent { get; set; } = false;

    [MaxLength(10)]
    public string Currency { get; set; } = "UAH"; // "UAH", "USD", "EUR"

    public bool NotifyInTelegram { get; set; } = true;
    public bool NotifyOnBooking { get; set; } = true;
    public bool NotifyOnFinance { get; set; } = true;
    public bool NotifyReminders { get; set; } = true;
    public bool NotifyOnNewFeedback { get; set; } = true;
}