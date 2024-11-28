using ErrorOr;
using MediatR;

namespace BuyTime_Application.Feedback.Command.Create;

public record CreateFeedbackCommand(
    Guid TeacherId,
    Guid StudentId,
    decimal Rating,
    string Comment,
    DateTime CreatedAt) : IRequest<ErrorOr<CreateFeedbackResult>>;