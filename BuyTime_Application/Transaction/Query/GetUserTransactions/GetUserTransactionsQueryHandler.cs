using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Transaction.Query.GetUserTransactions;

public class GetUserTransactionsQueryHandler(
    IUnitOfWork unitOfWork,
    ITonContractService tonContractService)
    : IRequestHandler<GetUserTransactionsQuery, ErrorOr<List<TransactionRecordDto>>>
{
    public async Task<ErrorOr<List<TransactionRecordDto>>> Handle(GetUserTransactionsQuery request, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Transactions.GetByUserIdAsync(request.UserId);

        if (result.IsError)
            return result.Errors;

        var dtos = result.Value.Adapt<List<TransactionRecordDto>>();

        foreach (var dto in dtos)
        {
            if (dto.Currency == "TON" && !string.IsNullOrEmpty(dto.ContractAddress))
            {
                dto.ExplorerUrl = tonContractService.GetExplorerUrl(dto.ContractAddress);
            }
        }

        return dtos;
    }
}