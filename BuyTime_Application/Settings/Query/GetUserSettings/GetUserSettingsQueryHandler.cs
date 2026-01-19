using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Domain.Entities;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Settings.Query.GetUserSettings;

public class GetUserSettingsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserSettingsQuery, ErrorOr<UserSettingsDto>>
{
    public async Task<ErrorOr<UserSettingsDto>> Handle(GetUserSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await unitOfWork.UserSettings.GetByUserIdAsync(request.UserId);

        if (settings == null)
        {
            return new UserSettingsDto
            {
                Theme = "Light",
                Language = "uk",
                Currency = "UAH",
                ShowCurrencyEquivalent = false,
                NotifyInTelegram = true,
                NotifyOnBooking = true,
                NotifyOnFinance = true,
                NotifyReminders = true,
                NotifyOnNewFeedback = true
            };
        }

        return settings.Adapt<UserSettingsDto>();
    }
}