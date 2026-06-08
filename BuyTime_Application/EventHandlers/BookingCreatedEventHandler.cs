using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Events;
using MediatR;

namespace BuyTime_Application.EventHandlers;

public class BookingCreatedEventHandler(ITelegramService telegramService, IUnitOfWork unitOfWork)
    : INotificationHandler<BookingCreatedEvent>
{
    public async Task Handle(BookingCreatedEvent notification, CancellationToken cancellationToken)
    {
        var booking = await unitOfWork.Booking.GetByIdAsync(notification.BookingId);
        if (booking == null) return;

        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(booking.TimeslotId);
        var expert = await unitOfWork.User.GetByIdAsync(timeslot.ExpertId);
        var student = await unitOfWork.User.GetByIdAsync(booking.StudentId);

        if (expert != null && !string.IsNullOrEmpty(expert.TelegramChatId))
        {
            var msg = $"<b>Нове бронювання!</b>\n" +
                      $"Студент: {student.FirstName} {student.LastName}\n" +
                      $"Час: {timeslot.StartTime} - {timeslot.EndTime}\n" +
                      $"Повідомлення: {booking.MessageToExpert}";

            await telegramService.SendMessageAsync(expert.TelegramChatId, msg);
        }
    }
}