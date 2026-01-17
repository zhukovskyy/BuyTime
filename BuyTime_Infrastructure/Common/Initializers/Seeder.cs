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

                // Застосовуємо міграції
                await context.Database.MigrateAsync();

                // ==========================================
                // 1. Сідінг Спеціалізацій (Specializations)
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
                // 2. Сідінг Користувачів (Users)
                // ==========================================
                if (!context.Users.Any())
                {
                    // Отримуємо спеціалізації з БД, щоб прив'язати їх до експертів
                    var mathSpec = await context.Specializations.FirstAsync(s => s.Name == "Математика");
                    var progSpec = await context.Specializations.FirstAsync(s => s.Name == "Програмування");
                    var designSpec = await context.Specializations.FirstAsync(s => s.Name == "Дизайн");
                    var marketingSpec = await context.Specializations.FirstAsync(s => s.Name == "Маркетинг");

                    var users = new List<User>
                    {
                        // Звичайні студенти
                        new User { Id = Guid.NewGuid(), FirstName = "Іван", LastName = "Шевченко", Email = "ivan.sh@example.com", TelegramChatId = "123456", IsExpert = false },
                        new User { Id = Guid.NewGuid(), FirstName = "Петро", LastName = "Безим'янний", Email = null, TelegramChatId = "999888", IsExpert = false },

                        // Експерт 1
                        new User
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "Андрій",
                            LastName = "Коваленко",
                            Email = "andriy.kovalenko@example.com",
                            TelegramChatId = "123456",
                            IsExpert = true,
                            ExpertNickname = "Коваленко Ментор",
                            Description = "Досвідчений викладач",
                            Rating = random.Next(4, 6), // 4 або 5
                            Specializations = new List<Specialization> { mathSpec, progSpec }
                        },
                        
                        // Експерт 2
                        new User
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "Марина",
                            LastName = "Данилова",
                            Email = "marina.danilova@example.com",
                            TelegramChatId = "123456",
                            IsExpert = true,
                            ExpertNickname = "Марина ІТ",
                            Description = "Сертифікований тренер",
                            Rating = random.Next(4, 6), // 4 або 5
                            Specializations = new List<Specialization> { designSpec, marketingSpec }
                        },
                    };
                    context.Users.AddRange(users);
                    await context.SaveChangesAsync();
                }

                // ==========================================
                // 3. Сідінг Таймслотів (Timeslots)
                // ==========================================
                if (!context.Timeslots.Any())
                {
                    var teachers = await context.Users.Where(u => u.IsExpert).ToListAsync();
                    var timeslots = new List<Timeslot>();

                    foreach (var teacher in teachers)
                    {
                        // Створимо по 5 слотів для кожного вчителя
                        for (int i = 0; i < 5; i++)
                        {
                            var startTime = DateTime.UtcNow.AddDays(random.Next(1, 10)).AddHours(random.Next(9, 18));
                            var endTime = startTime.AddHours(1);

                            // Випадкова валюта (TON або ETH)
                            var currency = random.Next(0, 2) == 0 ? "TON" : "ETH";
                            var price = currency == "TON" ? 10.0m + (i * 2) : 0.05m + (i * 0.01m);

                            timeslots.Add(new Timeslot
                            {
                                Id = Guid.NewGuid(),
                                ExpertId = teacher.Id,
                                StartTime = startTime,
                                EndTime = endTime,
                                IsAvailable = true, // Спочатку всі вільні
                                Price = price,
                                Currency = currency,
                                ExpertWalletAddress = "0x" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 40)
                            });
                        }
                    }

                    context.Timeslots.AddRange(timeslots);
                    await context.SaveChangesAsync();
                }

                // ==========================================
                // 4. Сідінг Бронювань (Bookings)
                // ==========================================
                if (!context.Bookings.Any())
                {
                    var students = await context.Users.Where(u => !u.IsExpert).ToListAsync();
                    var allTimeslots = await context.Timeslots.ToListAsync();
                    var bookings = new List<Booking>();

                    // Створимо кілька завершених бронювань, щоб були відгуки
                    foreach (var student in students)
                    {
                        // Беремо перший вільний слот
                        var slot = allTimeslots.FirstOrDefault(t => t.IsAvailable);
                        if (slot != null)
                        {
                            slot.IsAvailable = false; // Займаємо слот

                            bookings.Add(new Booking
                            {
                                Id = Guid.NewGuid(),
                                StudentId = student.Id,
                                TimeslotId = slot.Id,
                                Status = Status.Completed, // Робимо його завершеним
                                MessageToExpert = "Хочу забронювати цей урок.",
                                ContractHash = "fake_hash_" + Guid.NewGuid().ToString().Substring(0, 8),
                                ConfirmationMessage = "Ок, підтверджую",
                                Cancellation = null,
                                CreatedAt = DateTime.UtcNow.AddDays(-1), // Створено вчора
                                MeetingLink = "https://zoom.us/j/123123123",
                                StudentWalletAddress = "0xStudent" + Guid.NewGuid().ToString().Substring(0, 8)
                            });
                        }
                    }

                    context.Bookings.AddRange(bookings);
                    context.Timeslots.UpdateRange(allTimeslots); // Оновлюємо статус слотів
                    await context.SaveChangesAsync();
                }

                // ==========================================
                // 5. Сідінг Відгуків (Feedbacks)
                // ==========================================
                if (!context.Feedbacks.Any())
                {
                    // ВАЖЛИВО: Include(b => b.TimeSlot), щоб отримати ExpertId
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
                            StudentId = booking.StudentId,           // Хто пише
                            ExpertId = booking.TimeSlot.ExpertId,    // Кому пише (з слота)
                            Rating = (decimal)(random.Next(4, 6)),   // Хороші оцінки
                            Comment = "Чудовий урок, дякую!",
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    context.Feedbacks.AddRange(feedbacks);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}