using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Domain.Entities;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Wallet.Command.SetWalletByUserIdCommand;

public class SetWalletByUserIdCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SetWalletByUserIdCommand, ErrorOr<BuyTime_Domain.Entities.Wallet>>
{
    public async Task<ErrorOr<BuyTime_Domain.Entities.Wallet>> Handle(SetWalletByUserIdCommand request, CancellationToken cancellationToken)
    {
        var wallet = new BuyTime_Domain.Entities.Wallet
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            WalletType = request.WalletType,
            WalletAddress = request.WalletAddress
        };

        var result = await unitOfWork.Wallet.AddAsync(wallet);
        if (result.IsError)
            return result.Errors;

        await unitOfWork.CommitAsync();
        return wallet;
    }
}