using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Domain.Constants;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Command.ResolveByArbiter;

public class ResolveBookingByArbiterCommandHandler(
    IUnitOfWork unitOfWork,
    ITonContractService tonContractService)
    : IRequestHandler<ResolveBookingByArbiterCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(ResolveBookingByArbiterCommand request, CancellationToken cancellationToken)
    {
        var booking = await unitOfWork.Booking.GetByIdAsync(request.BookingId);

        if (booking == null || booking.TimeSlot == null)
            return Error.NotFound("Booking.NotFound", "Бронювання або таймслот не знайдено.");

        if (string.IsNullOrEmpty(booking.ContractAddress))
            return Error.Validation("Booking.NoContract", "У бронювання немає адреси смарт-контракту.");

        decimal price = booking.TimeSlot.Price;
        decimal studentAmount = 0;
        decimal expertAmount = 0;

        if (!request.IsExpertPresent)
        {
            studentAmount = price;
            expertAmount = 0;
            booking.Status = Status.Cancelled;
        }
        else
        {
            studentAmount = 0;
            expertAmount = price;
            booking.Status = Status.Completed;
        }

        var tonResult = await tonContractService.ResolveBookingByArbiterAsync(
            booking.ContractAddress,
            request.IsExpertPresent
        );

        if (tonResult.IsError)
            return tonResult.Errors;

        booking.Status = request.IsExpertPresent ? Status.Completed : Status.Refunded;
        await unitOfWork.Booking.UpdateAsync(booking);
        await unitOfWork.CommitAsync();

        return Unit.Value;
    }
}