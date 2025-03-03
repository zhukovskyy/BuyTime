using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Command.ToggleTeacher;

public class ToggleIsTeacherCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ToggleIsTeacherCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(ToggleIsTeacherCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await unitOfWork.User.GetByIdAsync(request.UserId);
            user.IsTeacher = !user.IsTeacher;
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