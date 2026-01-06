using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Infrastructure.Common.Persistence;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Repositories;

public class WalletRepository(BuyTimeDbContext context)
    : Repository<BuyTime_Domain.Entities.Wallet>(context), IWalletRepository
{
    public async Task<ErrorOr<List<BuyTime_Domain.Entities.Wallet>>> GetAllByUserIdAsync(Guid userId)
    {
        try
        {
            var wallets = await dbSet
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AddedAt)
                .ToListAsync();

            return wallets;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<BuyTime_Domain.Entities.Wallet>> GetByAddressAsync(string address, string network)
    {
        var wallet = await dbSet
            .FirstOrDefaultAsync(w => w.Address == address && w.Network == network);

        if (wallet == null) return Error.NotFound();

        return wallet;
    }

    public override async Task<BuyTime_Domain.Entities.Wallet?> GetByIdAsync(Guid id)
    {
        return await dbSet.FindAsync(id);
    }
}