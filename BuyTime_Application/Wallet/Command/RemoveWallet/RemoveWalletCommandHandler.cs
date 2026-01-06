using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Wallet.Command.RemoveWallet;

public class RemoveWalletCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveWalletCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(RemoveWalletCommand request, CancellationToken cancellationToken)
    {
        var wallet = await unitOfWork.Wallet.GetByIdAsync(request.WalletId);

        if (wallet == null)
            return Error.NotFound("Wallet not found");

        if (wallet.UserId != request.UserId)
            return Error.Validation("AccessDenied", "Ви не можете видалити чужий гаманець.");

        await unitOfWork.Wallet.DeleteAsync(wallet);
        await unitOfWork.CommitAsync();

        return Unit.Value;
    }
}