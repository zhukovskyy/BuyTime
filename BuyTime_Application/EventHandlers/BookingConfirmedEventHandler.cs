using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Events;
using MediatR;

namespace BuyTime_Application.EventHandlers;

public class BookingConfirmedEventHandler : INotificationHandler<BookingConfirmedEvent>
{
    private readonly ITelegramService _telegramService;
    private readonly IUnitOfWork _unitOfWork;

    public BookingConfirmedEventHandler(ITelegramService telegramService, IUnitOfWork unitOfWork)
    {
        _telegramService = telegramService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(BookingConfirmedEvent notification, CancellationToken cancellationToken)
    {
        var booking = await _unitOfWork.Booking.GetByIdAsync(notification.BookingId);
        if (booking == null) return;

        var student = await _unitOfWork.User.GetByIdAsync(booking.StudentId);

        var timeslot = await _unitOfWork.Timeslot.GetByIdAsync(booking.TimeslotId);
        var expert = await _unitOfWork.User.GetByIdAsync(timeslot.ExpertId);

        if (student != null && !string.IsNullOrEmpty(student.TelegramChatId))
        {
            // await _telegramService.SendMessageAsync(student.TelegramChatId, 
            //    $"Ваше бронювання підтверджено!\n" +
            //    $"Коментар: {notification.ConfirmationMessage}\n" +
            //    $"Посилання: {notification.MeetingLink}");
        }

        if (expert != null && !string.IsNullOrEmpty(expert.TelegramChatId))
        {
            // await _telegramService.SendMessageAsync(expert.TelegramChatId, 
            //    $"Ви підтвердили бронювання для студента {student?.FirstName}.");
        }
    }
}