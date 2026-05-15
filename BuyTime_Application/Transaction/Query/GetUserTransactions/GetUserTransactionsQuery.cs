using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Transaction.Query.GetUserTransactions;

public record GetUserTransactionsQuery(Guid UserId, string? Network) : IRequest<ErrorOr<List<TransactionRecordDto>>>;