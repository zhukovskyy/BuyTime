using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Command.ToggleExpert;

public class ToggleIsExpertCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ToggleIsExpertCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(ToggleIsExpertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await unitOfWork.User.GetByIdAsync(request.UserId);
            user.IsExpert = !user.IsExpert; // Expert
            await unitOfWork.User.UpdateAsync(user);
            await unitOfWork.CommitAsync();

            return Unit.Value;
        }
        catch (Exception ex)
        {
            return Error.Conflict(ex.Message);
        }
    }
}