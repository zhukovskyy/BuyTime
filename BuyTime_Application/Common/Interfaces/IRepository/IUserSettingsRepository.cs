using BuyTime_Domain.Entities;
using ErrorOr;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface IUserSettingsRepository : IRepository<UserSettings>
{
    Task<UserSettings?> GetByUserIdAsync(Guid userId);
}