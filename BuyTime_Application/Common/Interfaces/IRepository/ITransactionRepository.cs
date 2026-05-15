using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;
using ErrorOr;

public interface ITransactionRepository : IRepository<TransactionRecord>
{
    Task<ErrorOr<List<TransactionRecord>>> GetByUserIdAsync(Guid userId, string? network = null);
}