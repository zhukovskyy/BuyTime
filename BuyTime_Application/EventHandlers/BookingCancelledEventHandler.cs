using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Events;
using MediatR;

namespace BuyTime_Application.EventHandlers;
//  TODO: ОСКІЛЬКИ І СТУДЕНТ І ЕКСПЕРТ МОЖЕ ВІДМІНИТИ ЗУСТРІЧ, ТРЕБА БУДЕ РОЗДІЛИТИ ЛОГІКУ ПОВІДОМЛЕНЬ
public class BookingCancelledEventHandler(ITelegramService telegramService, IUnitOfWork unitOfWork)
    : INotificationHandler<BookingCancelledEvent>
{
    public async Task Handle(BookingCancelledEvent notification, CancellationToken cancellationToken)
    {
        var booking = await unitOfWork.Booking.GetByIdAsync(notification.BookingId);
        if (booking == null) return;

        var student = await unitOfWork.User.GetByIdAsync(booking.StudentId);

        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(booking.TimeslotId);
        var expert = await unitOfWork.User.GetByIdAsync(timeslot.ExpertId);

        if (student != null && !string.IsNullOrEmpty(student.TelegramChatId))
        {
            var msg = $"<b>Бронювання скасовано</b>\n" +
                      $"Причина: {notification.CancellationMessage}";
            // await telegramService.SendMessageAsync(student.TelegramChatId, msg);
        }

        if (expert != null && !string.IsNullOrEmpty(expert.TelegramChatId))
        {
            var msg = $"<b>Бронювання скасовано</b>\n" +
                      $"Студент: {student?.FirstName} {student?.LastName}\n" +
                      $"Причина: {notification.CancellationMessage}";
            // await telegramService.SendMessageAsync(expert.TelegramChatId, msg);
        }
    }
}