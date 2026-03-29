using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Repositories;

public class FeedbackRepository(BuyTimeDbContext context)
    : Repository<Feedback>(context), IFeedbackRepository
{
    public async Task<decimal> GetAverageRatingForExpertAsync(Guid expertId)
    {
        var ratings = await dbSet
            .Where(f => f.ExpertId == expertId)
            .Select(f => f.Rating)
            .ToListAsync();

        if (!ratings.Any()) return 0;

        return Math.Round(ratings.Average(), 2);
    }
}