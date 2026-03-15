using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using MediatR;
using ErrorOr;

namespace BuyTime_Application.Booking.Command.CreateBooking;

public class CreateBookingCommandHandler(
    IUnitOfWork unitOfWork,
    IBookingService bookingService,
    ITonContractService tonContractService)
    : IRequestHandler<CreateBookingCommand, ErrorOr<CreateBookingResult>>
{
    public async Task<ErrorOr<CreateBookingResult>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(request.TimeslotId);
        if (timeslot == null) return Error.NotFound("Timeslot.NotFound", "Таймслот не знайдено.");

        var studentWalletsResult = await unitOfWork.Wallet.GetAllByUserIdAsync(request.StudentId);
        if (studentWalletsResult.IsError)
            return studentWalletsResult.Errors;

        var studentWallet = studentWalletsResult.Value.FirstOrDefault(w => w.Network == timeslot.Currency);

        if (studentWallet == null)
            return Error.Validation("StudentWallet.Missing", $"У вас не прив'язаний гаманець для мережі {timeslot.Currency}. Будь ласка, додайте його в налаштуваннях гаманцч.");

        string studentWalletAddress = studentWallet.Address;

        var payloadResult = await tonContractService.GenerateCreateBookingPayloadAsync(
            studentWalletAddress,
            timeslot.ExpertWalletAddress,
            timeslot.StartTime,
            timeslot.EndTime,
            timeslot.Price
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