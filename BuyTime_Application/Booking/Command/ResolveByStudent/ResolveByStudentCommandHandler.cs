using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Domain.Constants;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Command.ResolveByStudent;

public class ResolveByStudentCommandHandler(
    IUnitOfWork unitOfWork,
    ITonContractService tonContractService
) : IRequestHandler<ResolveByStudentCommand, ErrorOr<ResolveByStudentResult>>
{
    public async Task<ErrorOr<ResolveByStudentResult>> Handle(ResolveByStudentCommand request, CancellationToken cancellationToken)
    {
        var booking = await unitOfWork.Booking.GetByIdAsync(request.BookingId);
        if (booking == null) return Error.NotFound("Booking.NotFound");
        if (booking.StudentId != request.StudentId) return Error.Unauthorized("Unauthorized");

        var arbiterResult = await tonContractService.ResolveBookingByArbiterAsync(booking.ContractAddress, request.IsSuccessful);

        if (arbiterResult.IsError) return arbiterResult.Errors;

        booking.Status = request.IsSuccessful ? Status.CompletionPending : Status.RefundPending;

        await unitOfWork.Booking.UpdateAsync(booking);
        await unitOfWork.CommitAsync();

        return new ResolveByStudentResult(true, "Транзакція відправлена. Очікуємо блокчейн...");
    }
}