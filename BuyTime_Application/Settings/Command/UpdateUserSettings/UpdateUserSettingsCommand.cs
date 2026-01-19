using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Settings.Command.UpdateUserSettings;

public record UpdateUserSettingsCommand(
    Guid UserId,
    string Theme,
    string Language,
    bool ShowCurrencyEquivalent,
    string Currency,
    bool NotifyInTelegram,
    bool NotifyOnBooking,
    bool NotifyOnFinance,
    bool NotifyReminders,
    bool NotifyOnNewFeedback
) : IRequest<ErrorOr<UserSettingsDto>>;