using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Repositories;

public class TransactionRepository(BuyTimeDbContext context)
    : Repository<TransactionRecord>(context), ITransactionRepository
{
    public async Task<ErrorOr<List<TransactionRecord>>> GetByUserIdAsync(Guid userId, string? network = null)
    {
        try
        {
            var query = dbSet
                .AsNoTracking()
                .Include(t => t.Booking)
                    .ThenInclude(b => b.TimeSlot)
                .Include(t => t.Booking)
                    .ThenInclude(b => b.Cancellation)
                .Where(t => t.UserId == userId);

            if (!string.IsNullOrEmpty(network))
            {
                query = query.Where(t => t.Currency == network);
            }

            var transactions = await query.OrderByDescending(t => t.ExecutedAt).ToListAsync();
            return transactions;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error retrieving transactions: {ex.Message}");
        }
    }
}