using ErrorOr;
using MediatR;

namespace BuyTime_Application.Wallet.Command.RemoveWallet;

public record RemoveWalletCommand(Guid UserId, Guid WalletId) : IRequest<ErrorOr<Unit>>;