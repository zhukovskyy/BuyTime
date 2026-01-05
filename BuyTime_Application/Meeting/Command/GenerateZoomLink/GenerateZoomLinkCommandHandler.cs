using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Meeting.Command.GenerateZoomLink;

public class GenerateZoomLinkCommandHandler(
    IUnitOfWork unitOfWork,
    IZoomService zoomService)
    : IRequestHandler<GenerateZoomLinkCommand, ErrorOr<string>>
{
    public async Task<ErrorOr<string>> Handle(GenerateZoomLinkCommand request, CancellationToken cancellationToken)
    {
        var booking = await unitOfWork.Booking.GetByIdAsync(request.BookingId);
        if (booking == null) return Error.NotFound("Booking not found");

        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(booking.TimeslotId);
        if (timeslot == null) return Error.NotFound("Timeslot not found");

        var duration = (int)(timeslot.EndTime - timeslot.StartTime).TotalMinutes;
        if (duration < 2) duration = 60; 

        var result = await zoomService.CreateMeetingAsync(
            topic: $"Lesson: {booking.Id}",
            startTime: timeslot.StartTime,
            durationMinutes: duration
        );

        return result;
    }
}