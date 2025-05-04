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
        var user = await _unitOfWork.User.GetByIdAsync(booking.UserId);

        if (user.TelegramChatId != null)
        {
            //await _telegramService.SendMessageAsync(user.TelegramChatId, 
                //$"Your booking has been confirmed! {notification.ConfirmationMessage} Contact link: {notification.ContactLink}");
        }
    }
}