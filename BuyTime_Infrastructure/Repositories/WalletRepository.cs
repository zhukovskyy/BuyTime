using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Repositories;

public class WalletRepository(BuyTimeDbContext context)
    : Repository<Wallet>(context), IWalletRepository
{
    public async Task<ErrorOr<BuyTime_Domain.Entities.Wallet>> GetByUserIdAsync(Guid id)
    {
        try
        {
            var wallet = await dbSet
                .Where(x => x.UserId == id)
                .FirstOrDefaultAsync();

            if (wallet == null)
            {
                return Error.NotFound("Wallet not found for the given user ID.");
            }

            return wallet;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving wallet: {ex.Message}");
        }
    }

    public async Task<ErrorOr<BuyTime_Domain.Entities.Wallet>> DeleteByUserIdAsync(Guid id)
    {
        try
        {
            var wallet = await dbSet
                .Where(x => x.UserId == id)
                .FirstOrDefaultAsync();

            if (wallet == null)
            {
                return Error.NotFound("Wallet not found for the given user ID.");
            }
            await DeleteAsync(wallet);
            return wallet;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while deleting wallet: {ex.Message}");
        }
    }

}