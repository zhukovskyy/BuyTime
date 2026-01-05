using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Command.ToggleExpert;

public record ToggleIsExpertCommand(Guid UserId) : IRequest<ErrorOr<Unit>>;