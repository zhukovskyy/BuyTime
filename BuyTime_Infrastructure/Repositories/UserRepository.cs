using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Application.Dto;
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

    public async Task<ErrorOr<User>> RegisterUserAsync(
    User userEntity,
    List<LanguageSkill> languages,
    List<SocialLinkDto> socialLinks,
    List<string> specializationNames)
    {
        try
        {
            var exists = await dbSet.AnyAsync(u => u.Id == userEntity.Id);
            if (exists)
            {
                return Error.Conflict("User.Exists", "Користувач з таким ID вже існує.");
            }

            if (languages != null)
            {
                foreach (var lang in languages)
                {
                    lang.Id = Guid.NewGuid();
                    lang.UserId = userEntity.Id; 
                                         
                    await context.Set<LanguageSkill>().AddAsync(lang);
                }
                
                userEntity.LanguageSkills = languages;
            }

            var allPlatforms = await context.SocialMediaPlatforms.ToListAsync();
            var expertLinks = new List<ExpertSocialLink>();

            if (socialLinks != null)
            {
                foreach (var linkDto in socialLinks)
                {
                    var platform = allPlatforms.FirstOrDefault(p =>
                        p.Name.Equals(linkDto.Platform, StringComparison.OrdinalIgnoreCase));

                    if (platform != null)
                    {
                        expertLinks.Add(new ExpertSocialLink
                        {
                            Id = Guid.NewGuid(),
                            ExpertId = userEntity.Id,
                            PlatformId = platform.Id,
                            UrlOrHandle = linkDto.UrlOrHandle
                        });
                    }
                }
                await context.ExpertSocialLinks.AddRangeAsync(expertLinks);
                userEntity.SocialLinks = expertLinks;
            }

            if (specializationNames != null && specializationNames.Any())
            {
                var specs = await context.Specializations
                    .Where(s => specializationNames.Contains(s.Name))
                    .ToListAsync();

                userEntity.Specializations ??= new List<Specialization>();

                foreach (var spec in specs)
                {
                    userEntity.Specializations.Add(spec);
                }
            }

            var defaultSettings = new UserSettings
            {
                Id = Guid.NewGuid(),
                UserId = userEntity.Id,
                Theme = "Light",
                Language = "uk",
                Currency = "UAH",
                NotifyInTelegram = true,
                NotifyOnBooking = true,
                NotifyOnFinance = true,
                NotifyReminders = true,
                NotifyOnNewFeedback = true
            };
            await context.UserSettings.AddAsync(defaultSettings);
            userEntity.Settings = defaultSettings;

            await dbSet.AddAsync(userEntity);

            await context.SaveChangesAsync();

            return userEntity;
        }
        catch (Exception ex)
        {
            return Error.Failure("CreateUserError", $"Не вдалося створити профіль: {ex.Message}");
        }
    }

    public async Task<ErrorOr<User>> UpdateUserProfileAsync(
    User userChanges,
    List<LanguageSkill> newLanguages,
    List<SocialLinkDto> newSocials,
    List<string> newSpecializationNames)
    {
        try
        {
            var user = await dbSet
                .Include(u => u.LanguageSkills)
                .Include(u => u.SocialLinks)
                .Include(u => u.Specializations) 
                .FirstOrDefaultAsync(u => u.Id == userChanges.Id);

            if (user == null)
                return Error.NotFound("User.NotFound", "Користувача не знайдено.");

            user.FirstName = userChanges.FirstName;
            user.LastName = userChanges.LastName;
            user.ExpertNickname = userChanges.ExpertNickname;
            user.Email = userChanges.Email;
            user.Description = userChanges.Description;
            user.AvatarUrl = userChanges.AvatarUrl;


            if (user.LanguageSkills != null && user.LanguageSkills.Any())
                context.Set<LanguageSkill>().RemoveRange(user.LanguageSkills);

            user.LanguageSkills = newLanguages;
            foreach (var lang in user.LanguageSkills) lang.UserId = user.Id;

            if (user.SocialLinks != null && user.SocialLinks.Any())
                context.ExpertSocialLinks.RemoveRange(user.SocialLinks);

            var newExpertLinks = new List<ExpertSocialLink>();
            var allPlatforms = await context.SocialMediaPlatforms.ToListAsync();

            foreach (var linkDto in newSocials)
            {
                var platform = allPlatforms.FirstOrDefault(p => p.Name.Equals(linkDto.Platform, StringComparison.OrdinalIgnoreCase));
                if (platform != null)
                {
                    newExpertLinks.Add(new ExpertSocialLink
                    {
                        Id = Guid.NewGuid(),
                        ExpertId = user.Id,
                        PlatformId = platform.Id,
                        UrlOrHandle = linkDto.UrlOrHandle
                    });
                }
            }
            user.SocialLinks = newExpertLinks;


            user.Specializations.Clear();

            if (newSpecializationNames != null && newSpecializationNames.Any())
            {
                var specsToAdd = await context.Specializations
                    .Where(s => newSpecializationNames.Contains(s.Name))
                    .ToListAsync();

                foreach (var spec in specsToAdd)
                {
                    user.Specializations.Add(spec);
                }
            }

            await context.SaveChangesAsync();

            return user;
        }
        catch (Exception ex)
        {
            return Error.Failure("UpdateError", $"Помилка: {ex.Message}");
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

            if (filter.OnlyFavorites && filter.CurrentUserId.HasValue)
            {
                query = query.Where(expert => context.FavoriteExperts
                    .Any(fe => fe.ExpertId == expert.Id && fe.StudentId == filter.CurrentUserId.Value));
            }

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