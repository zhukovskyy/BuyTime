using ErrorOr;
using MediatR;

namespace BuyTime_Application.Wallet.Command.DeleteWalletByUserIdCommand;

// disconnect wallet
public record DeleteWalletByUserIdCommand(Guid UserId)
    : IRequest<ErrorOr<Unit>>;
