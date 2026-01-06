using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Wallet.Query.GetUserWallets;

public record GetUserWalletsQuery(Guid UserId) : IRequest<ErrorOr<List<WalletDto>>>;