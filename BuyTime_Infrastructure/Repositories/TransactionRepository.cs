using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Repositories;

public class TransactionRepository(BuyTimeDbContext context)
    : Repository<TransactionRecord>(context), ITransactionRepository
{
    public async Task<ErrorOr<List<TransactionRecord>>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var transactions = await dbSet
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.ExecutedAt)
                .ToListAsync();

            return transactions;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error retrieving transactions: {ex.Message}");
        }
    }
}