using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Domain.Constants;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Command.ClaimRefund;

public class ClaimRefundCommandHandler(
    IUnitOfWork unitOfWork,
    ITonContractService tonContractService)
    : IRequestHandler<ClaimRefundCommand, ErrorOr<TonConnectPayloadDto>>
{
    public async Task<ErrorOr<TonConnectPayloadDto>> Handle(ClaimRefundCommand request, CancellationToken cancellationToken)
    {
        var booking = await unitOfWork.Booking.GetByIdAsync(request.BookingId);

        if (booking == null) return Error.NotFound("Booking.NotFound", "Бронювання не знайдено.");

        if (booking.StudentId != request.StudentId)
            return Error.Validation("AccessDenied", "Це не ваше бронювання.");

        if (booking.Status != Status.Rejected)
            return Error.Validation("InvalidStatus", "Повернути кошти можна лише для відхилених бронювань.");

        var payloadResult = await tonContractService.GenerateClaimRefundPayloadAsync(booking.ContractAddress);
        if (payloadResult.IsError) return payloadResult.Errors;

        booking.Status = Status.RefundPending;

        await unitOfWork.Booking.UpdateAsync(booking);
        await unitOfWork.CommitAsync();

        return payloadResult.Value;
    }
}