using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Wallet.Command.AddWallet;

public record AddWalletCommand(
    Guid UserId,
    string Network,
    string Address
) : IRequest<ErrorOr<WalletDto>>;