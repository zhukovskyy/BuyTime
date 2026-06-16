using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Domain.Constants;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Command.CancelBooking;

public class CancelBookingCommandHandler(
    IUnitOfWork unitOfWork,
    ITonContractService tonContractService)
    : IRequestHandler<CancelBookingCommand, ErrorOr<TonConnectPayloadDto>>
{
    public async Task<ErrorOr<TonConnectPayloadDto>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await unitOfWork.Booking.GetByIdAsync(request.BookingId);

        if (booking == null) return Error.NotFound("Booking.NotFound", "Бронювання не знайдено.");

        var uncancelableStatuses = new[] { Status.Completed, Status.Cancelled, Status.Refunded, Status.Rejected, Status.Expired };
        if (uncancelableStatuses.Contains(booking.Status))
        {
            return Error.Conflict("InvalidStatus", $"Неможливо скасувати бронювання зі статусом {booking.Status}.");
        }

        if (string.IsNullOrEmpty(booking.ContractAddress))
            return Error.Validation("Booking.NoContract", "У бронювання немає адреси смарт-контракту.");

        bool isStudent = request.TriggeredByUserId == booking.StudentId;

        ErrorOr<TonConnectPayloadDto> payloadResult;
        // Якщо експерт ще не підтвердив, а студент хоче скасувати зустріч, повертається 100% через арбітра
        if (isStudent && booking.Status == Status.Pending)
        {
            payloadResult = await tonContractService.GenerateClaimRefundPayloadAsync(booking.ContractAddress);
        }
        else
        {
            payloadResult = await tonContractService.GenerateCancelBookingPayloadAsync(isStudent, booking.ContractAddress);
        }

        if (payloadResult.IsError) return payloadResult.Errors;

        booking.Status = Status.CancelPending;

        bool wasConfirmed = !string.IsNullOrEmpty(booking.ConfirmationMessage) || !string.IsNullOrEmpty(booking.MeetingLink);
        TimeSpan timeBeforeMeeting = booking.TimeSlot.StartTime - DateTime.UtcNow;
        decimal fullPrice = booking.TimeSlot.Price;

        // TODO var expertSettings = await unitOfWork.UserSettings.GetByUserIdAsync(timeslot.ExpertId);

        decimal refundToStudent = 0;
        decimal compensationToExpert = 0;

        if (!isStudent || !wasConfirmed || timeBeforeMeeting.TotalHours >= 48)
        {
            // Скасував експерт АБО не підтверджено АБО більше 48 годин -> 100% повернення студенту
            refundToStudent = fullPrice;
        }
        else if (timeBeforeMeeting.TotalHours < 48 && timeBeforeMeeting.TotalHours >= 24)
        {
            // Від 24 до 48 годин -> 50/50
            refundToStudent = Math.Round(fullPrice / 2m, 9); // Округлення для TON
            compensationToExpert = fullPrice - refundToStudent;
        }
        else
        {
            // Менше 24 годин -> 100% йде експерту
            compensationToExpert = fullPrice;
        }
        // ==========================================

        booking.Cancellation = new BuyTime_Domain.Entities.BookingCancellation
        {
            BookingId = request.BookingId,
            Reason = request.CancellationMessage,
            CancelledAt = DateTime.UtcNow,
            CancelledByUserId = request.TriggeredByUserId,
            RefundAmountToStudent = refundToStudent,
            CompensationAmountToExpert = compensationToExpert
        };

        await unitOfWork.Booking.UpdateAsync(booking);
        await unitOfWork.CommitAsync();

        return payloadResult.Value;
    }
}