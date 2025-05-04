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

                await context.Database.MigrateAsync();

                if (!context.Users.Any())
                {
                    var users = new List<User>
                    {
                        new User { Id = Guid.NewGuid(), FirstName = "Іван", LastName = "Шевченко", Email = "ivan.sh@example.com", TelegramChatId = "123456", IsTeacher = false },
                        new User { Id = Guid.NewGuid(), FirstName = "Ольга", LastName = "Іванова", Email = "olga.ivanova@example.com", TelegramChatId = "123456", IsTeacher = false },
                        new User { Id = Guid.NewGuid(), FirstName = "Андрій", LastName = "Коваленко", Email = "andriy.kovalenko@example.com", TelegramChatId = "123456", IsTeacher = true, TeacherNickname = "Коваленко Репетитор", Description = "Досвідчений викладач.", Rating = random.Next(1, 6), Tags = "математика, програмування" },
                        new User { Id = Guid.NewGuid(), FirstName = "Марина", LastName = "Данилова", Email = "marina.danilova@example.com", TelegramChatId = "123456", IsTeacher = true, TeacherNickname = "Марина ІТ", Description = "Сертифікований тренер.", Rating = random.Next(1, 6), Tags = "дизайн, маркетинг" },
                    };

                    context.Users.AddRange(users);
                    await context.SaveChangesAsync();
                }

                if (!context.Timeslots.Any())
                {
                    var teachers = context.Users.Where(u => u.IsTeacher).ToList();
                    var timeslots = new List<Timeslot>();
                    foreach (var teacher in teachers)
                    {
                        for (int i = 0; i < 2; i++) 
                        {
                            var startTime = DateTime.Now.AddHours(random.Next(1, 5));
                            var endTime = startTime.AddHours(1);
                            timeslots.Add(new Timeslot
                            {
                                Id = Guid.NewGuid(),
                                UserId = teacher.Id,
                                StartTime = startTime,
                                EndTime = endTime,
                                IsAvailable = true
                            });
                        }
                    }

                    context.Timeslots.AddRange(timeslots);
                    await context.SaveChangesAsync();
                }

                if (!context.Bookings.Any())
                {
                    var students = context.Users.Where(u => !u.IsTeacher).ToList();
                    var timeslots = context.Timeslots.ToList();
                    var bookings = new List<Booking>();

                    foreach (var student in students)
                    {
                        var randomTimeslot = timeslots[random.Next(timeslots.Count)];
                        bookings.Add(new Booking
                        {
                            Id = Guid.NewGuid(),
                            UserId = student.Id,
                            TimeslotId = randomTimeslot.Id,
                            Status = Status.Pending,
                            Message = "Хочу забронювати цей урок.",
                            CreatedAt = DateTime.Now,
                            UrlOfMeeting = null
                        });
                    }

                    context.Bookings.AddRange(bookings);
                    await context.SaveChangesAsync();
                }

                if (!context.Feedbacks.Any())
                {
                    var bookings = context.Bookings.ToList();
                    var feedbacks = new List<Feedback>();

                    foreach (var booking in bookings)
                    {
                        feedbacks.Add(new Feedback
                        {
                            Id = Guid.NewGuid(),
                            UserId = booking.UserId,
                            Rating = (decimal)(random.Next(1, 6)),
                            Comment = "Чудовий урок!",
                            CreatedAt = DateTime.Now
                        });
                    }

                    context.Feedbacks.AddRange(feedbacks);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
