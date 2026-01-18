using ErrorOr;
using MediatR;

namespace BuyTime_Application.Student.Command.ToggleFavorite;

public record ToggleFavoriteCommand(Guid StudentId, Guid ExpertId) : IRequest<ErrorOr<bool>>;
