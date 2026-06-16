using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Common.Settings;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Options;

namespace BuyTime_Application.Timeslot.Command.UpdateTimeslot;

public class UpdateTimeslotCommandHandler(IUnitOfWork unitOfWork,
    IOptions<PlatformSettings> platformSettings)
    : IRequestHandler<UpdateTimeslotCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(UpdateTimeslotCommand request, CancellationToken cancellationToken)
    {
        if (request.Currency == "TON" && request.Price < platformSettings.Value.MinTimeslotPriceTon)
        {
            return Error.Validation("InvalidPrice", $"Мінімальна вартість таймслота — {platformSettings.Value.MinTimeslotPriceTon} TON.");
        }

        if (request.StartTime >= request.EndTime)
        {
            return Error.Validation("InvalidTime", "Час початку має бути раніше за час закінчення.");
        }

        if (request.StartTime < DateTime.UtcNow)
        {
            return Error.Validation("InvalidTime", "Неможливо перенести таймслот у минуле.");
        }

        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(request.TimeslotId);

        if (timeslot == null)
        {
            return Error.NotFound("Timeslot.NotFound", "Таймслот не знайдено.");
        }

        if (timeslot.ExpertId != request.ExpertId)
        {
            return Error.Validation("AccessDenied", "Ви не можете редагувати чужий таймслот.");
        }

        if (!timeslot.IsAvailable)
        {
            return Error.Conflict("TimeslotBooked", "Неможливо редагувати таймслот, який вже заброньовано.");
        }

        bool hasOverlap = await unitOfWork.Timeslot.HasOverlappingAsync(request.ExpertId, request.StartTime, request.EndTime, request.TimeslotId);
        if (hasOverlap)
        {
            return Error.Conflict("Timeslot.Overlap", "Вибраний час перетинається з іншим вашим таймслотом.");
        }

        if (timeslot.Currency != request.Currency)
        {
            var walletsResult = await unitOfWork.Wallet.GetAllByUserIdAsync(request.ExpertId);

            if (walletsResult.IsError)
                return walletsResult.Errors;

            var matchingWallet = walletsResult.Value.FirstOrDefault(w => w.Network == request.Currency);

            if (matchingWallet == null)
            {
                return Error.Conflict("ExpertWalletMissing", $"Ви повинні прив'язати {request.Currency} гаманець в профілі перед тим як змінювати валюту.");
            }

            timeslot.ExpertWalletAddress = matchingWallet.Address;
        }

        timeslot.StartTime = request.StartTime;
        timeslot.EndTime = request.EndTime;
        timeslot.Price = request.Price;
        timeslot.Currency = request.Currency;

        await unitOfWork.Timeslot.UpdateAsync(timeslot);
        await unitOfWork.CommitAsync();

        return Unit.Value;
    }
}