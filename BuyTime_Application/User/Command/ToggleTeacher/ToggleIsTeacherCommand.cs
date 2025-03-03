using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Command.ToggleTeacher;

public record ToggleIsTeacherCommand(Guid UserId) : IRequest<ErrorOr<Unit>>;