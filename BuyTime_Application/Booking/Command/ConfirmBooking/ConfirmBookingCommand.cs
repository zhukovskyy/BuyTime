using MediatR;
using ErrorOr;

namespace BuyTime_Application.Booking.Command.ConfirmBooking;

public record ConfirmBookingCommand(
    Guid BookingId,
    Guid ExpertId,
    string ConfirmationMessage,
    string? MeetingLink,      // вручну введене посилання
    string? MeetingTitle,     // назва консультації/зустрічі
    bool GenerateMeetingLink  // якщо стоїть чекбокс/тоггл на примусове генерування
) : IRequest<ErrorOr<Unit>>;