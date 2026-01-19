using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Domain.Entities;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Settings.Command.UpdateUserSettings;

public class UpdateUserSettingsCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUserSettingsCommand, ErrorOr<UserSettingsDto>>
{
    public async Task<ErrorOr<UserSettingsDto>> Handle(UpdateUserSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await unitOfWork.UserSettings.GetByUserIdAsync(request.UserId);

        if (settings == null)
        {
            settings = new UserSettings
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId
            };
            await unitOfWork.UserSettings.AddAsync(settings);
        }

        settings.Theme = request.Theme;
        settings.Language = request.Language;
        settings.ShowCurrencyEquivalent = request.ShowCurrencyEquivalent;
        settings.Currency = request.Currency;

        settings.NotifyInTelegram = request.NotifyInTelegram;
        settings.NotifyOnBooking = request.NotifyOnBooking;
        settings.NotifyOnFinance = request.NotifyOnFinance;
        settings.NotifyReminders = request.NotifyReminders;
        settings.NotifyOnNewFeedback = request.NotifyOnNewFeedback;

        await unitOfWork.UserSettings.UpdateAsync(settings);
        await unitOfWork.CommitAsync();

        return settings.Adapt<UserSettingsDto>();
    }
}