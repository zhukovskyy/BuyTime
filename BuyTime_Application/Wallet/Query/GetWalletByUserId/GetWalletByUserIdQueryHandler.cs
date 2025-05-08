using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Domain.Entities;
using ErrorOr;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BuyTime_Application.Wallet.Query;

public class GetWalletByUserIdQueryHandler (IUnitOfWork unitOfWork)
    : IRequestHandler<GetWalletByUserIdQuery, ErrorOr<WalletDto>>
{
    public async Task<ErrorOr<WalletDto>> Handle(GetWalletByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var walletResult = await unitOfWork.Wallet.GetByUserIdAsync(request.UserId);

            if (walletResult.IsError)
            {
                return Error.Failure("Wallet not found");
            }

            var walletDto = walletResult.Value.Adapt<WalletDto>();
            
            return walletDto;
        }
        catch (Exception ex)
        {
            return Error.Failure("Error: " + ex.Message);
        }
    }
}