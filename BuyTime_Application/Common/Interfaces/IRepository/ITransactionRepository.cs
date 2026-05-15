using BuyTime_Domain.Entities;
using ErrorOr;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface ITransactionRepository : IRepository<TransactionRecord>
{
    Task<ErrorOr<List<TransactionRecord>>> GetByUserIdAsync(Guid userId);
}