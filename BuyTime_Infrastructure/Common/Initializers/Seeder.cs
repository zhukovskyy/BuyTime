using BuyTime_Domain.Entities;
using BuyTime_Domain.Constants;
using BuyTime_Infrastructure.Common.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuyTime_Infrastructure.Common.Initializers
{
    public static class Seeder
    {
        public static async void SeedData(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var service = scope.ServiceProvider;
                Random random = new Random();
                var context = service.GetRequiredService<BuyTimeDbContext>();

                // Ensure tracking is on to avoid duplicates when attaching existing entities
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                await context.Database.MigrateAsync();

                // ==========================================
                // 1. Seed Specializations
                // ==========================================
                if (!context.Specializations.Any())
                {
                    var specs = new List<Specialization>
                    {
                        new Specialization { Id = Guid.NewGuid(), Name = "Математика" },
                        new Specialization { Id = Guid.NewGuid(), Name = "Програмування" },
                        new Specialization { Id = Guid.NewGuid(), Name = "Дизайн" },
                        new Specialization { Id = Guid.NewGuid(), Name = "Маркетинг" },
                        new Specialization { Id = Guid.NewGuid(), Name = "Англійська мова" }
                    };
                    context.Specializations.AddRange(specs);
                    await context.SaveChangesAsync();
                }

                // ==========================================
                // 2. Seed Languages
                // ==========================================
                if (!context.Languages.Any())
                {
                    var languages = new List<Language>
                    {
                        new Language { Id = Guid.NewGuid(), Code = "uk" },
                        new Language { Id = Guid.NewGuid(), Code = "en" },
                        new Language { Id = Guid.NewGuid(), Code = "de" },
                        new Language { Id = Guid.NewGuid(), Code = "pl" },
                        new Language { Id = Guid.NewGuid(), Code = "es" },
                        new Language { Id = Guid.NewGuid(), Code = "fr" }
                    };
                    context.Languages.AddRange(languages);
                    await context.SaveChangesAsync();
                }

                // ==========================================
                // 3. Seed Social Media Platforms
                // ==========================================
                if (!context.SocialMediaPlatforms.Any())
                {
                    var platforms = new List<SocialMediaPlatform>
                    {
                        new SocialMediaPlatform { Id = Guid.NewGuid(), Name = "LinkedIn", LogoUrl = "assets/icons/linkedin.png" },
                        new SocialMediaPlatform { Id = Guid.NewGuid(), Name = "Telegram", LogoUrl = "assets/icons/telegram.png" },
                        new SocialMediaPlatform { Id = Guid.NewGuid(), Name = "Instagram", LogoUrl = "assets/icons/instagram.png" },
                        new SocialMediaPlatform { Id = Guid.NewGuid(), Name = "Facebook", LogoUrl = "assets/icons/facebook.png" }
                    };
                    context.SocialMediaPlatforms.AddRange(platforms);
                    await context.SaveChangesAsync();
                }

                // ==========================================
                // 4. Seed Users (Safe Method)
                // ==========================================
                if (!context.Users.Any())
                {
                    // 1. Fetch referenced entities (IDs are what matter most)
                    var mathSpec = await context.Specializations.FirstAsync(s => s.Name == "Математика");
                    var progSpec = await context.Specializations.FirstAsync(s => s.Name == "Програмування");
                    var designSpec = await context.Specializations.FirstAsync(s => s.Name == "Дизайн");
                    var marketingSpec = await context.Specializations.FirstAsync(s => s.Name == "Маркетинг");

                    var ukrLang = await context.Languages.FirstAsync(l => l.Code == "uk");
                    var engLang = await context.Languages.FirstAsync(l => l.Code == "en");

                    var linkedin = await context.SocialMediaPlatforms.FirstAsync(p => p.Name == "LinkedIn");
                    var telegram = await context.SocialMediaPlatforms.FirstAsync(p => p.Name == "Telegram");

                    // 2. Create Users WITHOUT relationships first
                    var student1 = new User { Id = Guid.NewGuid(), FirstName = "Іван", LastName = "Шевченко", Email = "ivan.sh@example.com", TelegramChatId = "123456", IsExpert = false };
                    var student2 = new User { Id = Guid.NewGuid(), FirstName = "Петро", LastName = "Безим'янний", Email = null, TelegramChatId = "999888", IsExpert = false };

                    var expert1 = new User
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Андрій",
                        LastName = "Коваленко",
                        Email = "andriy.kovalenko@example.com",
                        TelegramChatId = "123456",
                        IsExpert = true,
                        ExpertNickname = "Коваленко Ментор",
                        Description = "Досвідчений викладач",
                        Rating = 5
                        // No Specializations or Languages yet
                    };

                    var expert2 = new User
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Марина",
                        LastName = "Данилова",
                        Email = "marina.danilova@example.com",
                        TelegramChatId = "123456",
                        IsExpert = true,
                        ExpertNickname = "Марина ІТ",
                        Description = "Сертифікований тренер",
                        Rating = 5
                        // No Specializations or Languages yet
                    };

                    // 3. Save Users to DB
                    context.Users.AddRange(student1, student2, expert1, expert2);
                    await context.SaveChangesAsync();

                    // 4. Now ADD RELATIONSHIPS to the tracked entities

                    // Expert 1 Relationships
                    if (expert1.Specializations == null) expert1.Specializations = new List<Specialization>();
                    expert1.Specializations.Add(mathSpec);
                    expert1.Specializations.Add(progSpec);

                    await context.Set<ExpertLanguage>().AddRangeAsync(
                        new ExpertLanguage { ExpertId = expert1.Id, LanguageId = ukrLang.Id, Level = "Native" },
                        new ExpertLanguage { ExpertId = expert1.Id, LanguageId = engLang.Id, Level = "C1" }
                    );

                    // Expert 2 Relationships
                    if (expert2.Specializations == null) expert2.Specializations = new List<Specialization>();
                    expert2.Specializations.Add(designSpec);
                    expert2.Specializations.Add(marketingSpec);

                    await context.Set<ExpertLanguage>().AddAsync(
                        new ExpertLanguage { ExpertId = expert2.Id, LanguageId = ukrLang.Id, Level = "Native" }
                    );

                    await context.Set<ExpertSocialLink>().AddRangeAsync(
                        new ExpertSocialLink { Id = Guid.NewGuid(), ExpertId = expert2.Id, PlatformId = linkedin.Id, UrlOrHandle = "https://linkedin.com/in/marina" },
                        new ExpertSocialLink { Id = Guid.NewGuid(), ExpertId = expert2.Id, PlatformId = telegram.Id, UrlOrHandle = "@marina_it" }
                    );

                    // 5. Final Save
                    await context.SaveChangesAsync();
                }

                // ==========================================
                // 5. User Settings
                // ==========================================
                if (!context.UserSettings.Any())
                {
                    var allUsers = await context.Users.ToListAsync();
                    var settingsList = new List<UserSettings>();

                    foreach (var user in allUsers)
                    {
                        settingsList.Add(new UserSettings
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            Theme = "Light",
                            Language = "uk",
                            Currency = "UAH",
                            ShowCurrencyEquivalent = false,
                            NotifyInTelegram = true,
                            NotifyOnBooking = true,
                            NotifyOnFinance = true,
                            NotifyReminders = true,
                            NotifyOnNewFeedback = true
                        });
                    }
                    context.UserSettings.AddRange(settingsList);
                    await context.SaveChangesAsync();
                }

                // ==========================================
                // 6. Timeslots
                // ==========================================
                if (!context.Timeslots.Any())
                {
                    var teachers = await context.Users.Where(u => u.IsExpert).ToListAsync();
                    var timeslots = new List<Timeslot>();

                    foreach (var teacher in teachers)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            var startTime = DateTime.UtcNow.AddDays(random.Next(1, 10)).AddHours(random.Next(9, 18));
                            var endTime = startTime.AddHours(1);
                            var currency = random.Next(0, 2) == 0 ? "TON" : "ETH";
                            var price = currency == "TON" ? 10.0m + (i * 2) : 0.05m + (i * 0.01m);
                            var fakeAddress = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);

                            timeslots.Add(new Timeslot
                            {
                                Id = Guid.NewGuid(),
                                ExpertId = teacher.Id,
                                StartTime = startTime,
                                EndTime = endTime,
                                IsAvailable = true,
                                Price = price,
                                Currency = currency,
                                ExpertWalletAddress = "0x" + fakeAddress
                            });
                        }
                    }
                    context.Timeslots.AddRange(timeslots);
                    await context.SaveChangesAsync();
                }

                // ==========================================
                // 7. Bookings
                // ==========================================
                if (!context.Bookings.Any())
                {
                    var students = await context.Users.Where(u => !u.IsExpert).ToListAsync();
                    var allTimeslots = await context.Timeslots.ToListAsync();
                    var bookings = new List<Booking>();

                    foreach (var student in students)
                    {
                        var slot = allTimeslots.FirstOrDefault(t => t.IsAvailable);
                        if (slot != null)
                        {
                            slot.IsAvailable = false;
                            bookings.Add(new Booking
                            {
                                Id = Guid.NewGuid(),
                                StudentId = student.Id,
                                TimeslotId = slot.Id,
                                Status = Status.Completed,
                                MessageToExpert = "Хочу забронювати цей урок.",
                                ContractAddress = "fake_hash_" + Guid.NewGuid().ToString().Substring(0, 8),
                                ConfirmationMessage = "Ок, підтверджую",
                                Cancellation = null,
                                CreatedAt = DateTime.UtcNow.AddDays(-1),
                                MeetingLink = "https://zoom.us/j/123123123",
                                StudentWalletAddress = "0xStudent" + Guid.NewGuid().ToString().Substring(0, 8)
                            });
                        }
                    }
                    context.Bookings.AddRange(bookings);
                    context.Timeslots.UpdateRange(allTimeslots);
                    await context.SaveChangesAsync();
                }

                // ==========================================
                // 8. Feedbacks
                // ==========================================
                if (!context.Feedbacks.Any())
                {
                    var completedBookings = await context.Bookings
                        .Include(b => b.TimeSlot)
                        .Where(b => b.Status == Status.Completed)
                        .ToListAsync();

                    var feedbacks = new List<Feedback>();

                    foreach (var booking in completedBookings)
                    {
                        feedbacks.Add(new Feedback
                        {
                            Id = Guid.NewGuid(),
                            StudentId = booking.StudentId,
                            ExpertId = booking.TimeSlot.ExpertId,
                            Rating = (decimal)(random.Next(4, 6)),
                            Comment = "Чудовий урок, дякую!",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    context.Feedbacks.AddRange(feedbacks);
                    await context.SaveChangesAsync();
                }

                // ==========================================
                // 9. Blockchain Data
                // ==========================================
                if (!context.BlockchainData.Any())
                {
                    var blockchainData = new List<BlockchainData>
                    {
                        new BlockchainData
                        {
                            Id = Guid.NewGuid(),
                            Name = "PlatformWallet",
                            Address = "0QAi1uwqjwAkBPUPhfF6Guk8Qi6O6xQ-LKcdzBLHY1pJE3OR",
                            Mnemonic = null
                        },
                        new BlockchainData
                        {
                            Id = Guid.NewGuid(),
                            Name = "ArbiterWallet",
                            Address = "0xARBITER_ADDRESS_PLACEHOLDER_FOR_DEV",
                            Mnemonic = "word1 word2 word3 word4 word5 word6 word7 word8 word9 word10 word11 word12"
                        }
                    };
                    context.BlockchainData.AddRange(blockchainData);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}