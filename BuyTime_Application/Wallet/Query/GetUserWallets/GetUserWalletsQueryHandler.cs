using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Wallet.Query.GetUserWallets;

public class GetUserWalletsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserWalletsQuery, ErrorOr<List<WalletDto>>>
{
    public async Task<ErrorOr<List<WalletDto>>> Handle(GetUserWalletsQuery request, CancellationToken cancellationToken)
    {
        var walletsResult = await unitOfWork.Wallet.GetAllByUserIdAsync(request.UserId);

        if (walletsResult.IsError)
            return walletsResult.Errors;

        return walletsResult.Value.Adapt<List<WalletDto>>();
    }
}