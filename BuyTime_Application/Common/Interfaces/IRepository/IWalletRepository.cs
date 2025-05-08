using ErrorOr;
using MediatR;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface IWalletRepository : IRepository<BuyTime_Domain.Entities.Wallet>
{
    Task<ErrorOr<BuyTime_Domain.Entities.Wallet>> GetByUserIdAsync(Guid id);
    Task<ErrorOr<BuyTime_Domain.Entities.Wallet>> DeleteByUserIdAsync(Guid id);
}