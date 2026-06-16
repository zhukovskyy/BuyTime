using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Common.Settings;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Options;

namespace BuyTime_Application.Booking.Command.CreateBooking;

public class CreateBookingCommandHandler(
    IUnitOfWork unitOfWork,
    IBookingService bookingService,
    ITonContractService tonContractService,
    IOptions<PlatformSettings> platformSettings)
    : IRequestHandler<CreateBookingCommand, ErrorOr<CreateBookingResult>>
{
    public async Task<ErrorOr<CreateBookingResult>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(request.TimeslotId);
        if (timeslot == null) return Error.NotFound("Timeslot.NotFound", "Таймслот не знайдено.");

        if (!timeslot.IsAvailable)
            return Error.Conflict("TimeslotBooked", "Таймслот вже зайнято.");

        var studentWalletsResult = await unitOfWork.Wallet.GetAllByUserIdAsync(request.StudentId);
        if (studentWalletsResult.IsError)
            return studentWalletsResult.Errors;

        var studentWallet = studentWalletsResult.Value.FirstOrDefault(w => w.Network == timeslot.Currency);

        if (studentWallet == null)
            return Error.Validation("StudentWallet.Missing", $"У вас не прив'язаний гаманець для мережі {timeslot.Currency}. Будь ласка, додайте його в налаштуваннях гаманцч.");

        string studentWalletAddress = studentWallet.Address;
        decimal commissionPercent = platformSettings.Value.CommissionPercent;
        decimal multiplier = 1m - (commissionPercent / 100m);
        decimal totalAmountToPay = timeslot.Price / multiplier;
        totalAmountToPay = Math.Round(totalAmountToPay, 9);

        var existingBookings = await unitOfWork.Booking.GetBookingsByTimeSlotIdAsync(request.TimeslotId);
        if (!existingBookings.IsError && existingBookings.Value.Any(b =>
            b.Status != BuyTime_Domain.Constants.Status.Cancelled &&
            b.Status != BuyTime_Domain.Constants.Status.Rejected))
        {
            return Error.Conflict("TimeslotBooked", "Таймслот щойно був заброньований кимось іншим.");
        }

        var payloadResult = await tonContractService.GenerateCreateBookingPayloadAsync(
            studentWalletAddress,
            timeslot.ExpertWalletAddress,
            timeslot.StartTime,
            timeslot.EndTime,
            totalAmountToPay
        );

        if (payloadResult.IsError) return payloadResult.Errors;

        var payload = payloadResult.Value;

        var bookingId = await bookingService.CreateBookingAsync(
            studentId: request.StudentId,
            timeslotId: request.TimeslotId,
            messageToExpert: request.MessageToExpert,
            contractAddress: payload.ContractAddress,
            studentWalletAddress: studentWalletAddress
        );

        return new CreateBookingResult
        {
            BookingId = bookingId,
            TonPayload = payload
        };
    }
}