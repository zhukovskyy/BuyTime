using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Wallet.Query;

public record GetWalletByUserIdQuery(Guid UserId) : IRequest<ErrorOr<WalletDto>>;