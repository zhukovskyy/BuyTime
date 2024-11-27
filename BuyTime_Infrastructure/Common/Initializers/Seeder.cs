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
                        new User { Id = Guid.NewGuid(), FirstName = "Іван", LastName = "Шевченко", Role = Roles.Student, Email = "ivan.sh@example.com" },
                        new User { Id = Guid.NewGuid(), FirstName = "Ольга", LastName = "Іванова", Role = Roles.Student, Email = "olga.ivanova@example.com" },
                        new User { Id = Guid.NewGuid(), FirstName = "Андрій", LastName = "Коваленко", Role = Roles.Teacher, Email = "andriy.kovalenko@example.com" },
                        new User { Id = Guid.NewGuid(), FirstName = "Марина", LastName = "Данилова", Role = Roles.Teacher, Email = "marina.danilova@example.com" },
                        new User { Id = Guid.NewGuid(), FirstName = "Тарас", LastName = "Гончаренко", Role = Roles.Student, Email = "taras.goncharenko@example.com" },
                        new User { Id = Guid.NewGuid(), FirstName = "Наталія", LastName = "Петренко", Role = Roles.Student, Email = "natalia.petrenko@example.com" },
                        new User { Id = Guid.NewGuid(), FirstName = "Дмитро", LastName = "Сидоренко", Role = Roles.Teacher, Email = "dmytro.sydorenko@example.com" },
                        new User { Id = Guid.NewGuid(), FirstName = "Анна", LastName = "Левченко", Role = Roles.Student, Email = "anna.levchenko@example.com" },
                        new User { Id = Guid.NewGuid(), FirstName = "Павло", LastName = "Мельник", Role = Roles.Teacher, Email = "pavlo.melnik@example.com" },
                        new User { Id = Guid.NewGuid(), FirstName = "Юлія", LastName = "Бойко", Role = Roles.Student, Email = "yulia.boyko@example.com" }
                    };

                    context.Users.AddRange(users);
                    await context.SaveChangesAsync();
                }

                if (!context.Teachers.Any())
                {
                    var teachers = context.Users.Where(u => u.Role == Roles.Teacher).ToList();
                    foreach (var teacher in teachers)
                    {
                        var teacherEntity = new Teacher
                        {
                            Id = teacher.Id,
                            FirstName = teacher.FirstName,
                            LastName = teacher.LastName,
                            Role = teacher.Role,
                            Email = teacher.Email,
                            Description = "Досвідчений викладач з великим досвідом роботи.",
                            Rating = random.Next(1, 6),
                            Tags = "математика, програмування"
                        };
                        context.Teachers.Add(teacherEntity);
                    }
                    await context.SaveChangesAsync();
                }

                if (!context.Timeslots.Any())
                {
                    var teachers = context.Teachers.ToList();
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
                                TeacherId = teacher.Id,
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
                    var users = context.Users.Where(u => u.Role == Roles.Student).ToList();
                    var timeslots = context.Timeslots.ToList();
                    var bookings = new List<Booking>();

                    foreach (var user in users)
                    {
                        var randomTimeslot = timeslots[random.Next(timeslots.Count)];
                        bookings.Add(new Booking
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            TeacherId = randomTimeslot.TeacherId,
                            TimeslotId = randomTimeslot.Id,
                            Status = Status.Pending,
                            Message = "Хочу забронювати цей урок.",
                            CreatedAt = DateTime.Now
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
                            TeacherId = booking.TeacherId,
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
