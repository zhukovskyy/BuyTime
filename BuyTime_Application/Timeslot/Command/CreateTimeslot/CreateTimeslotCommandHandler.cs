using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Common.Settings;
using BuyTime_Application.Timeslot.Command.CreateTimeslot;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Options;

namespace BuyTime_Application.Timeslot.CreateTimeslot;

public class CreateTimeslotCommandHandler(
    IUnitOfWork unitOfWork,
    IOptions<PlatformSettings> platformSettings)
    : IRequestHandler<CreateTimeslotCommand, ErrorOr<CreateTimeslotResult>>
{
    public async Task<ErrorOr<CreateTimeslotResult>> Handle(CreateTimeslotCommand request,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.User.GetByIdAsync(request.ExpertId);

        if (user == null)
        {
            return Error.NotFound("User.NotFound", "Користувача не знайдено.");
        }

        if (!user.IsExpert)
        {
            return Error.Forbidden("AccessDenied", "Створення таймслотів доступне виключно для користувачів з роллю Експерта.");
        }

        if (request.Currency == "TON" && request.Price < platformSettings.Value.MinTimeslotPriceTon)
        {
            return Error.Validation("InvalidPrice", $"Мінімальна вартість таймслота — {platformSettings.Value.MinTimeslotPriceTon} TON.");
        }

        if (request.StartTime >= request.EndTime)
        {
            return Error.Failure("Invalid timeslot. Start time must be before end time.");
        }

        if (request.StartTime < DateTime.UtcNow)
        {
            return Error.Validation("InvalidTime", "Час початку таймслоту не може бути в минулому.");
        }

        bool hasOverlap = await unitOfWork.Timeslot.HasOverlappingAsync(request.ExpertId, request.StartTime, request.EndTime);
        if (hasOverlap)
        {
            // TODO: передавати інфу про той створений слот
            return Error.Conflict("Timeslot.Overlap", "У вас вже є створений таймслот, який перетинається з цим часом.");
        }

        var walletsResult = await unitOfWork.Wallet.GetAllByUserIdAsync(request.ExpertId);

        if (walletsResult.IsError)
        {
            return walletsResult.Errors;
        }

        var matchingWallet = walletsResult.Value
            .FirstOrDefault(w => w.Network == request.Currency);

        if (matchingWallet == null)
        {
            return Error.Conflict("ExpertWalletMissing",
                $"Ви повинні прив'язати {request.Currency} гаманець в профілі перед тим як створювати таймслот.");
        }

        var timeslot = new BuyTime_Domain.Entities.Timeslot
        {
            Id = Guid.NewGuid(),
            ExpertId = request.ExpertId, 
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsAvailable = true,

            Price = request.Price,
            Currency = request.Currency,
            ExpertWalletAddress = matchingWallet.Address
        };

        await unitOfWork.Timeslot.AddAsync(timeslot);
        await unitOfWork.CommitAsync();

        return new CreateTimeslotResult
        {
            TimeslotId = timeslot.Id,
            ExpertId = timeslot.ExpertId,
            StartTime = timeslot.StartTime,
            EndTime = timeslot.EndTime,
            IsAvailable = timeslot.IsAvailable,
            Price = timeslot.Price,
            Currency = timeslot.Currency
        };
    }
}