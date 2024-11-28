using MediatR;

namespace BuyTime_Application.Events;

public class BookingConfirmedEvent(Guid bookingId, string confirmationMessage, string contactLink)
    : INotification
{
    public Guid BookingId { get; set; } = bookingId;
    public string ConfirmationMessage { get; set; } = confirmationMessage;
    public string ContactLink { get; set; } = contactLink;
}