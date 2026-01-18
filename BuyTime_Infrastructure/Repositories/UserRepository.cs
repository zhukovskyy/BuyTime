using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Application.Expert.Query.Search;
using BuyTime_Domain.Constants;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Repositories;

public class UserRepository(BuyTimeDbContext context)
    : Repository<User>(context), IUserRepository
{
    public async Task<ErrorOr<User>> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await dbSet
                .Include(u => u.LanguageSkills) 
                .Include(u => u.SocialLinks)
                    .ThenInclude(sl => sl.Platform)
                .Include(u => u.Specializations)
                .FirstOrDefaultAsync(user => user.Id == id);
            if (user == null)
                return Error.NotFound("User not found");
            return user;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }
    public async Task<ErrorOr<User>> GetUserByChatIdAsync(string chatId)
    {
        try
        {
            var user = await dbSet.FirstOrDefaultAsync(user => user.TelegramChatId == chatId);
            if (user == null)
                return Error.NotFound("User not found");
            return user;
           
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<IEnumerable<User>>> GetAllStudentsAsync()
    {
        try
        {
            var students = await dbSet.Where(user => user.IsExpert == false).ToListAsync();
            return students;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<IEnumerable<User>>> GetAllExpertsAsync()
    {
        try
        {
            var experts = await dbSet
                                .Where(u => u.IsExpert == true)
                                .Include(u => u.TimeSlots)
                                .Include(u => u.LanguageSkills) 
                                .Include(u => u.SocialLinks)
                                    .ThenInclude(sl => sl.Platform)
                                .Include(u => u.Specializations)
                                .ToListAsync();
            return experts;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<IEnumerable<User>>> GetAllUsersAsync()
    {
        try
        {
            var users = await dbSet.ToListAsync();
            return users;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<User>> AddUserDetailsAsync(User user)
    {
        try
        {
            await dbSet.AddAsync(user);
            await context.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            return Error.Failure("Error while adding user details");
        }
    }

    public async Task<ErrorOr<IEnumerable<User>>> SearchExpertsAsync(SearchExpertRequest filter)
    {
        try
        {
            var query = dbSet.AsNoTracking()
                .Where(u => u.IsExpert)
                .Include(u => u.LanguageSkills)
                .Include(u => u.SocialLinks)
                    .ThenInclude(sl => sl.Platform)
                .Include(u => u.ReceivedFeedbacks)
                .Include(u => u.Specializations)

                .Include(u => u.TimeSlots.Where(ts =>
                    (ts.Booking != null && ts.Booking.Status == Status.Completed) ||
                    (ts.IsAvailable && (string.IsNullOrEmpty(filter.Currency) || ts.Currency == filter.Currency))
                ))
                .ThenInclude(ts => ts.Booking)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                var q = filter.SearchQuery.Trim().ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(q) ||
                    u.LastName.ToLower().Contains(q) ||
                    (u.ExpertNickname != null && u.ExpertNickname.ToLower().Contains(q)) ||
                    (u.FirstName + " " + u.LastName).ToLower().Contains(q)
                );
            }

            // =================================================================================
            // Мова (ЛОГІКА AND: Експерт має знати ВСІ перелічені мови)
            // =================================================================================
            if (!string.IsNullOrWhiteSpace(filter.Language))
            {
                var languages = filter.Language
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Distinct() 
                    .ToList();

                foreach (var lang in languages)
                {
                    query = query.Where(u => u.LanguageSkills.Any(l => l.LanguageName == lang));
                }
            }

            // =================================================================================
            // Спеціалізація (ЛОГІКА AND: Експерт повинен мати ВСІ перелічені спеціалізації)
            // =================================================================================
            if (!string.IsNullOrWhiteSpace(filter.Specialization))
            {
                var searchSpecs = filter.Specialization.ToLower()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Distinct()
                    .ToList();

                // Аналогічно: фільтруємо послідовно.
                // Якщо запит "дизайн, маркетинг", то спочатку відберемо тих, у кого є "дизайн",
                // а потім з них залишимо тільки тих, у кого є ще й "маркетинг".
                foreach (var searchSpec in searchSpecs)
                {
                    query = query.Where(u => u.Specializations.Any(s => s.Name.ToLower().Contains(searchSpec)));
                }
            }

            // Рейтинг (без змін)
            if (filter.MinRating.HasValue && filter.MinRating.Value > 0)
            {
                query = query.Where(u => u.Rating >= filter.MinRating.Value);
            }

            // Валюта та Ціна (без змін)
            if (!string.IsNullOrEmpty(filter.Currency))
            {
                query = query.Where(u =>
                    u.TimeSlots.Any(ts => ts.IsAvailable && ts.Currency == filter.Currency));

                if (filter.MaxAveragePriceForFilter.HasValue && filter.MaxAveragePriceForFilter.Value > 0)
                {
                    query = query.Where(u =>
                        u.TimeSlots
                            .Where(ts => ts.IsAvailable && ts.Currency == filter.Currency)
                            .Average(ts => ts.Price) <= filter.MaxAveragePriceForFilter.Value
                    );
                }
            }

            var experts = await query.ToListAsync();
            return experts;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Search failed: {ex.Message}");
        }
    }
}