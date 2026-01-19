using ErrorOr;
using MediatR;

namespace BuyTime_Application.Feedback.Command.Create;

public record CreateFeedbackCommand(
    Guid StudentId, 
    Guid ExpertId,  
    decimal Rating,
    string? Comment) : IRequest<ErrorOr<CreateFeedbackResult>>;