using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using MediatR;
using ErrorOr;

namespace BuyTime_Application.Booking.Command.ConfirmBooking;

public class ConfirmBookingCommandHandler(
    IUnitOfWork unitOfWork,
    IBookingService bookingService,
    IZoomService zoomService) 
    : IRequestHandler<ConfirmBookingCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        // TODO: зробити зміщення UTC по локалі, бо зараз по базі UTC+0, або це фронтенд має робити скоріше всього
        var booking = await unitOfWork.Booking.GetByIdAsync(request.BookingId);
        if (booking == null) return Error.NotFound("Booking not found");

        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(booking.TimeslotId);
        if (timeslot == null) return Error.NotFound("Timeslot not found");

        string finalMeetingLink = request.MeetingLink?.Trim() ?? string.Empty;

        bool shouldGenerateLink = request.GenerateMeetingLink || string.IsNullOrEmpty(finalMeetingLink);

        if (shouldGenerateLink)
        {
            string topic;
            if (!string.IsNullOrWhiteSpace(request.MeetingTitle))
            {
                topic = request.MeetingTitle;
            }
            else
            {
                topic = $"Зустріч: {timeslot.StartTime:dd.MM.yyyy HH:mm}";
            }

            var duration = (int)(timeslot.EndTime - timeslot.StartTime).TotalMinutes;
            if (duration < 2) duration = 60;

            var zoomResult = await zoomService.CreateMeetingAsync(
                topic: topic,
                startTime: timeslot.StartTime,
                durationMinutes: duration
            );

            if (zoomResult.IsError)
            {
                return zoomResult.Errors;
            }

            finalMeetingLink = zoomResult.Value;
        }

        //if (string.IsNullOrEmpty(finalMeetingLink))
        //{
        //    return Error.Validation("MeetingLink", "Meeting link is required (enter manually or check generate option).");
        //}

        return await bookingService.ConfirmBookingAsync(
            request.BookingId,
            request.ConfirmationMessage,
            finalMeetingLink
        );
    }
}