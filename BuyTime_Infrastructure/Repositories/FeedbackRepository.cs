using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;

namespace BuyTime_Infrastructure.Repositories;

public class FeedbackRepository(BuyTimeDbContext context)
    : Repository<Feedback>(context), IFeedbackRepository
{
}