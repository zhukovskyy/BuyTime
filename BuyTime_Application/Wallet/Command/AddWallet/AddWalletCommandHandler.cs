using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Domain.Entities;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Wallet.Command.AddWallet;

public class AddWalletCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<AddWalletCommand, ErrorOr<WalletDto>>
{
    public async Task<ErrorOr<WalletDto>> Handle(AddWalletCommand request, CancellationToken cancellationToken)
    {
        var userWallets = await unitOfWork.Wallet.GetAllByUserIdAsync(request.UserId);

        if (!userWallets.IsError)
        {
            var hasWalletInNetwork = userWallets.Value.Any(w => w.Network == request.Network);
            if (hasWalletInNetwork)
            {
                return Error.Conflict("WalletLimit", $"У вас вже підключений гаманець {request.Network}. Видаліть старий перед додаванням нового.");
            }
        }

        var wallet = new BuyTime_Domain.Entities.Wallet
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Network = request.Network,
            Address = request.Address,
            AddedAt = DateTime.UtcNow
        };

        await unitOfWork.Wallet.AddAsync(wallet);
        await unitOfWork.CommitAsync();

        return wallet.Adapt<WalletDto>();
    }
}