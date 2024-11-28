using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Feedback.Command.Create;

public class CreateFeedbackCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateFeedbackCommand, ErrorOr<CreateFeedbackResult>>
{
    public async Task<ErrorOr<CreateFeedbackResult>> Handle(CreateFeedbackCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var feedback = new BuyTime_Domain.Entities.Feedback
            {
                TeacherId = request.TeacherId,
                UserId = request.UserId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = request.CreatedAt
            };

            var result = await unitOfWork.Feedback.AddAsync(feedback);

            if (result.IsError)
            {
                return Error.Failure("FeedbackCreationError", "Failed to create feedback");
            }

            await unitOfWork.CommitAsync();

            return new CreateFeedbackResult(result.Value);
        }
        catch (InvalidOperationException ex)
        {
            return Error.Validation("FeedbackCreationError", ex.Message);
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }
}