using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Services;

public class BlockchainService(BuyTimeDbContext context) : IBlockchainService
{
    public async Task<string> GetPlatformAddressAsync()
    {
        var data = await context.BlockchainData
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == "PlatformWallet");

        if (data == null || string.IsNullOrEmpty(data.Address))
        {
            throw new Exception("Platform wallet address not configured in database.");
        }

        return data.Address;
    }

    public async Task<string> GetArbiterMnemonicAsync()
    {
        var data = await context.BlockchainData
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == "ArbiterWallet");

        if (data == null || string.IsNullOrEmpty(data.Mnemonic))
        {
            throw new Exception("Arbiter mnemonic not configured in database.");
        }

        return data.Mnemonic;
    }

    public async Task<string> GetArbiterAddressAsync()
    {
        var data = await context.BlockchainData
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == "ArbiterWallet");

        if (data == null || string.IsNullOrEmpty(data.Address))
        {
            throw new Exception("Arbiter address not configured in database.");
        }

        return data.Address;
    }
}