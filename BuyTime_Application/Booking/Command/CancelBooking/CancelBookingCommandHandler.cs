using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Command.CancelBooking;

public class CancelBookingCommandHandler(
    IUnitOfWork unitOfWork,
    IBookingService bookingService,
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

        var payloadResult = await tonContractService.GenerateCancelBookingPayloadAsync(isStudent, booking.ContractAddress);

        if (payloadResult.IsError) return payloadResult.Errors;

        var dbResult = await bookingService.CancelBookingAsync(
            request.BookingId,
            request.CancellationMessage,
            request.TriggeredByUserId
        );

        if (dbResult.IsError) return dbResult.Errors;

        return payloadResult.Value;
    }
}