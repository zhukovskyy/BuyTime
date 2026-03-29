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

        booking.Cancellation = new BuyTime_Domain.Entities.BookingCancellation
        {
            BookingId = request.BookingId,
            Reason = request.CancellationMessage,
            CancelledAt = DateTime.UtcNow,
            CancelledByUserId = request.TriggeredByUserId
        };

        await unitOfWork.Booking.UpdateAsync(booking);
        await unitOfWork.CommitAsync();

        return payloadResult.Value;
    }
}