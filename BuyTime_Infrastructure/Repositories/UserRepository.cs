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
                .Include(u => u.ExpertLanguages)
                    .ThenInclude(ls => ls.Language)
                .Include(u => u.SocialLinks)
                    .ThenInclude(sl => sl.Platform)
                .Include(u => u.Specializations)
                .Include(u => u.ReceivedFeedbacks)
                .Include(u => u.TimeSlots)
                .Include(u => u.Bookings)
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

    public async Task<ErrorOr<UserProfileDto>> GetUserProfileAsync(Guid id)
    {
        try
        {
            var profile = await dbSet
                .Where(u => u.Id == id)
                .Select(u => new UserProfileDto
                {
                    Id = u.Id,
                    IsExpert = u.IsExpert,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    AvatarUrl = u.AvatarUrl,
                    TelegramChatId = u.TelegramChatId,
                    DiscordId = u.DiscordId,
                    Email = u.Email,

                    ExpertNickname = u.ExpertNickname,
                    Description = u.Description,
                    Rating = u.Rating,

                    // SQL COUNT
                    ReviewCount = u.ReceivedFeedbacks.Count(),
                    HappyStudentsCount = u.ReceivedFeedbacks.Count(f => f.Rating >= 4),

                    // SQL SUM + DATEDIFF
                    TotalHoursConducted = u.TimeSlots
                        .Where(ts => ts.Bookings.Any(b => b.Status == Status.Completed))
                        .Sum(ts => (double)EF.Functions.DateDiffMinute(ts.StartTime, ts.EndTime) / 60.0),

                    // Проекції списків
                    Specializations = u.Specializations.Select(s => new SpecializationDto
                    {
                        Id = s.Id,
                        Name = s.Name
                    }).ToList(),

                    LanguageSkills = u.ExpertLanguages.Select(l => new LanguageSkillDto
                    {
                        LanguageCode = l.Language.Code,
                        Level = l.Level
                    }).ToList(),

                    SocialLinks = u.SocialLinks.Select(sl => new SocialLinkDto
                    {
                        Platform = sl.Platform.Name,
                        LogoUrl = sl.Platform.LogoUrl,
                        UrlOrHandle = sl.UrlOrHandle
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (profile == null)
                return Error.NotFound("User.NotFound", "Користувача не знайдено");

            return profile;
        }
        catch (Exception ex)
        {
            return Error.Failure("Database.Error", ex.Message);
        }
    }

    public async Task<ErrorOr<User>> RegisterUserAsync(
        User userEntity,
        List<LanguageSkillDto> languageDtos,
        List<SocialLinkDto> socialLinks,
        List<string> specializationNames)
    {
        try
        {
            var exists = await dbSet.AnyAsync(u => u.Id == userEntity.Id);
            if (exists)
                return Error.Conflict("User.Exists", "Користувач вже існує.");

            if (languageDtos != null && languageDtos.Any())
            {
                var allDbLanguages = await context.Languages.AsTracking().ToListAsync();
                var expertLanguages = new List<ExpertLanguage>();

                foreach (var dto in languageDtos)
                {
                    var lang = allDbLanguages.FirstOrDefault(l =>
                        l.Code.Equals(dto.LanguageCode, StringComparison.OrdinalIgnoreCase));

                    if (lang != null)
                    {
                        expertLanguages.Add(new ExpertLanguage
                        {
                            ExpertId = userEntity.Id,
                            LanguageId = lang.Id,
                            Language = lang,
                            Level = dto.Level
                        });
                    }
                }
                userEntity.ExpertLanguages = expertLanguages;
            }

            if (socialLinks != null && socialLinks.Any())
            {
                var allPlatforms = await context.SocialMediaPlatforms.AsTracking().ToListAsync();
                var expertLinks = new List<ExpertSocialLink>();

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
                            Platform = platform,
                            UrlOrHandle = linkDto.UrlOrHandle
                        });
                    }
                }
                userEntity.SocialLinks = expertLinks;
            }

            if (specializationNames != null && specializationNames.Any())
            {
                var specs = await context.Specializations
                    .AsTracking()
                    .Where(s => specializationNames.Contains(s.Name))
                    .ToListAsync();

                userEntity.Specializations = specs;
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
            userEntity.Settings = defaultSettings;

            await dbSet.AddAsync(userEntity);
            await context.SaveChangesAsync();

            return userEntity;
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            return Error.Failure("RegisterUserError", msg);
        }
    }

    public async Task<ErrorOr<User>> UpdateUserProfileAsync(
    User userChanges,
    List<LanguageSkillDto> languageDtos,
    List<SocialLinkDto> newSocials,
    List<string> newSpecializationNames)
    {
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // 1. Завантажуємо юзера
            var user = await dbSet
                .AsTracking()
                .Include(u => u.Specializations)
                .FirstOrDefaultAsync(u => u.Id == userChanges.Id);

            if (user == null)
                return Error.NotFound("User.NotFound", "Користувача не знайдено.");

            // 2. Оновлюємо скалярні поля
            user.FirstName = userChanges.FirstName;
            user.LastName = userChanges.LastName;
            user.ExpertNickname = userChanges.ExpertNickname;
            user.Email = userChanges.Email;
            user.DiscordId = userChanges.DiscordId;
            user.Description = userChanges.Description;
            user.AvatarUrl = userChanges.AvatarUrl;

            // =========================================================
            // ЕТАП 1: МОВИ
            // =========================================================
            var oldLanguages = await context.ExpertLanguages
                .Where(el => el.ExpertId == user.Id)
                .ToListAsync();

            if (oldLanguages.Any()) context.ExpertLanguages.RemoveRange(oldLanguages);

            var currentExpertLanguages = new List<ExpertLanguage>();

            if (languageDtos != null && languageDtos.Any())
            {
                var allDbLanguages = await context.Languages.AsNoTracking().ToListAsync();
                foreach (var dto in languageDtos)
                {
                    var lang = allDbLanguages.FirstOrDefault(l =>
                        l.Code.Equals(dto.LanguageCode, StringComparison.OrdinalIgnoreCase));

                    if (lang != null)
                    {
                        currentExpertLanguages.Add(new ExpertLanguage
                        {
                            ExpertId = user.Id,
                            LanguageId = lang.Id,
                            Level = dto.Level,
                            Language = null
                        });
                    }
                }
                await context.ExpertLanguages.AddRangeAsync(currentExpertLanguages);
            }

            // =========================================================
            // ЕТАП 2: СОЦМЕРЕЖІ
            // =========================================================
            var oldSocials = await context.ExpertSocialLinks
                .Where(sl => sl.ExpertId == user.Id)
                .ToListAsync();

            if (oldSocials.Any()) context.ExpertSocialLinks.RemoveRange(oldSocials);

            var currentSocialLinks = new List<ExpertSocialLink>();

            if (newSocials != null && newSocials.Any())
            {
                var allPlatforms = await context.SocialMediaPlatforms.AsNoTracking().ToListAsync();
                foreach (var linkDto in newSocials)
                {
                    var platform = allPlatforms.FirstOrDefault(p =>
                        p.Name.Equals(linkDto.Platform, StringComparison.OrdinalIgnoreCase));

                    if (platform != null)
                    {
                        currentSocialLinks.Add(new ExpertSocialLink
                        {
                            Id = Guid.NewGuid(),
                            ExpertId = user.Id,
                            PlatformId = platform.Id,
                            UrlOrHandle = linkDto.UrlOrHandle,
                            Platform = null
                        });
                    }
                }
                await context.ExpertSocialLinks.AddRangeAsync(currentSocialLinks);
            }

            // =========================================================
            // ЕТАП 3: СПЕЦІАЛІЗАЦІЇ (ВИПРАВЛЕНО)
            // =========================================================

            if (newSpecializationNames != null)
            {
                // 1. Видаляємо ті, яких більше немає в списку
                // Використовуємо .ToList(), щоб створити копію для ітерації
                var specsToRemove = user.Specializations
                    .Where(s => !newSpecializationNames.Contains(s.Name))
                    .ToList();

                foreach (var specToRemove in specsToRemove)
                {
                    user.Specializations.Remove(specToRemove);
                }

                // 2. Додаємо нові, яких ще немає в юзера
                var currentSpecNames = user.Specializations.Select(s => s.Name).ToList();
                var namesToAdd = newSpecializationNames
                    .Except(currentSpecNames) // Тільки ті, що нові
                    .ToList();

                if (namesToAdd.Any())
                {
                    // Завантажуємо тільки ті, яких не вистачає
                    var specsToAdd = await context.Specializations
                        .Where(s => namesToAdd.Contains(s.Name))
                        .ToListAsync();

                    foreach (var spec in specsToAdd)
                    {
                        user.Specializations.Add(spec);
                    }
                }
            }
            else
            {
                // Якщо прийшов null, очищаємо все
                user.Specializations.Clear();
            }

            // Зберігаємо зміни
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            // =========================================================
            // ЕТАП 4: ВІДНОВЛЕННЯ ДЛЯ DTO
            // =========================================================

            if (currentExpertLanguages.Any())
            {
                var allDbLanguages = await context.Languages.AsNoTracking().ToListAsync();
                foreach (var item in currentExpertLanguages)
                {
                    item.Language = allDbLanguages.First(l => l.Id == item.LanguageId);
                }
            }

            if (currentSocialLinks.Any())
            {
                var allPlatforms = await context.SocialMediaPlatforms.AsNoTracking().ToListAsync();
                foreach (var item in currentSocialLinks)
                {
                    item.Platform = allPlatforms.First(p => p.Id == item.PlatformId);
                }
            }

            user.ExpertLanguages = currentExpertLanguages;
            user.SocialLinks = currentSocialLinks;

            return user;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            var msg = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            return Error.Failure("UpdateError", $"Помилка: {msg}");
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
                                .Include(u => u.ExpertLanguages)
                                    .ThenInclude(el => el.Language)
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
                .Include(u => u.ExpertLanguages)
                    .ThenInclude(el => el.Language)
                .Include(u => u.SocialLinks)
                    .ThenInclude(sl => sl.Platform)
                .Include(u => u.ReceivedFeedbacks)
                .Include(u => u.Specializations)

                .Include(u => u.TimeSlots.Where(ts =>
                    (ts.Bookings != null && ts.Bookings.Any(b=>b.Status == Status.Completed)) ||
                    (ts.IsAvailable && (string.IsNullOrEmpty(filter.Currency) || ts.Currency == filter.Currency))
                ))
                .ThenInclude(ts => ts.Bookings)
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
            // Мова 
            // =================================================================================
            if (!string.IsNullOrWhiteSpace(filter.Language))
            {
                var languages = filter.Language
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim().ToLower()) 
                    .Distinct()
                    .ToList();

                foreach (var langCode in languages)
                {
                    query = query.Where(u => u.ExpertLanguages.Any(l => l.Language.Code == langCode));
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Specialization))
            {
                var searchSpecs = filter.Specialization.ToLower()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Distinct()
                    .ToList();

                foreach (var searchSpec in searchSpecs)
                {
                    query = query.Where(u => u.Specializations.Any(s => s.Name.ToLower().Contains(searchSpec)));
                }
            }

            if (filter.MinRating.HasValue && filter.MinRating.Value > 0)
            {
                query = query.Where(u => u.Rating >= filter.MinRating.Value);
            }

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