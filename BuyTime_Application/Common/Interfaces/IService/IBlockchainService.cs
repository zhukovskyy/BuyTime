namespace BuyTime_Application.Common.Interfaces.IService;

public interface IBlockchainService
{
    Task<string> GetPlatformAddressAsync();
    Task<string> GetArbiterAddressAsync();
    Task<string> GetArbiterMnemonicAsync();
}