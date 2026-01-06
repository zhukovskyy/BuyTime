using ErrorOr;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface IWalletRepository : IRepository<BuyTime_Domain.Entities.Wallet>
{
    Task<ErrorOr<List<BuyTime_Domain.Entities.Wallet>>> GetAllByUserIdAsync(Guid userId);
    Task<ErrorOr<BuyTime_Domain.Entities.Wallet>> GetByAddressAsync(string address, string network);
}