using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using MediatR;
using ErrorOr;

namespace BuyTime_Application.Booking.Command.ConfirmBooking;

public class ConfirmBookingCommandHandler(
    IUnitOfWork unitOfWork,
    IBookingService bookingService,
    IDiscordService discordService) 
    : IRequestHandler<ConfirmBookingCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await unitOfWork.Booking.GetByIdAsync(request.BookingId);
        if (booking == null) return Error.NotFound("Booking not found");

        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(booking.TimeslotId);
        if (timeslot == null) return Error.NotFound("Timeslot not found");

        var student = await unitOfWork.User.GetByIdAsync(booking.StudentId);
        var expert = await unitOfWork.User.GetByIdAsync(timeslot.ExpertId);

        string finalMeetingLink = request.MeetingLink?.Trim() ?? string.Empty;
        bool shouldGenerateLink = request.GenerateMeetingLink || string.IsNullOrEmpty(finalMeetingLink);

        if (shouldGenerateLink)
        {
            string topic = !string.IsNullOrWhiteSpace(request.MeetingTitle)
                ? request.MeetingTitle
                : $"Зустріч: {timeslot.StartTime:dd.MM.yyyy HH:mm}";

            var discordIds = new List<string>();
            if (!string.IsNullOrEmpty(student?.DiscordId)) discordIds.Add(student.DiscordId);
            if (!string.IsNullOrEmpty(expert?.DiscordId)) discordIds.Add(expert.DiscordId);

            if (discordIds.Count == 0)
            {
                return Error.Validation("DiscordIdMissing", "Neither the student nor the expert has a linked Discord ID.");
            }

            var discordResult = await discordService.CreateMeetingAsync(topic, discordIds);

            if (discordResult.IsError)
            {
                return discordResult.Errors;
            }

            finalMeetingLink = discordResult.Value;
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