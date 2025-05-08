using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using ErrorOr;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BuyTime_Application.Wallet.Command.DeleteWalletByUserIdCommand;

// disconnect wallet
public class DeleteWalletByUserIdCommandHandler (IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteWalletByUserIdCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(DeleteWalletByUserIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var wallet = await unitOfWork.Wallet.GetByUserIdAsync(request.UserId);
            if (wallet.IsError)
            {
                return Error.NotFound("Wallet not found.");
            }
            var result = await unitOfWork.Wallet.DeleteByUserIdAsync(request.UserId);
            if (result.IsError)
            {
                return result.Errors;
            }
            await unitOfWork.CommitAsync();
            return Unit.Value;
        }
        catch (Exception ex)
        {
            return Error.Failure("Error: " + ex.Message);
        }
        
    }
}