using ErrorOr;
using MediatR;

namespace BuyTime_Application.Wallet.Command.SetWalletByUserIdCommand;

public record SetWalletByUserIdCommand(Guid UserId, string WalletType, string WalletAddress)
    : IRequest<ErrorOr<BuyTime_Domain.Entities.Wallet>>;