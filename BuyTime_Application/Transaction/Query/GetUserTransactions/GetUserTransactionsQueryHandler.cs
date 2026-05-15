using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Transaction.Query.GetUserTransactions;

public class GetUserTransactionsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserTransactionsQuery, ErrorOr<List<TransactionRecordDto>>>
{
    public async Task<ErrorOr<List<TransactionRecordDto>>> Handle(GetUserTransactionsQuery request, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Transactions.GetByUserIdAsync(request.UserId);

        if (result.IsError)
            return result.Errors;

        return result.Value.Adapt<List<TransactionRecordDto>>();
    }
}