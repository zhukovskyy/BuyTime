using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Repositories;

public class FavoriteExpertRepository(BuyTimeDbContext context)
    : IFavoriteExpertRepository
{
    public async Task<FavoriteExpert?> GetAsync(Guid studentId, Guid expertId)
    {
        return await context.FavoriteExperts
            .FirstOrDefaultAsync(fe => fe.StudentId == studentId && fe.ExpertId == expertId);
    }

    public async Task<HashSet<Guid>> GetExpertIdsByStudentIdAsync(Guid studentId)
    {
        var ids = await context.FavoriteExperts
            .Where(fe => fe.StudentId == studentId)
            .Select(fe => fe.ExpertId)
            .ToListAsync();

        return new HashSet<Guid>(ids);
    }

    public async Task AddAsync(FavoriteExpert entity)
    {
        await context.FavoriteExperts.AddAsync(entity);
    }

    public Task DeleteAsync(FavoriteExpert entity)
    {
        context.FavoriteExperts.Remove(entity);
        return Task.CompletedTask;
    }
}