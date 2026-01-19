using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Repositories;

public class UserSettingsRepository(BuyTimeDbContext context)
    : Repository<UserSettings>(context), IUserSettingsRepository
{
    public async Task<UserSettings?> GetByUserIdAsync(Guid userId)
    {
        return await dbSet.FirstOrDefaultAsync(s => s.UserId == userId);
    }
}