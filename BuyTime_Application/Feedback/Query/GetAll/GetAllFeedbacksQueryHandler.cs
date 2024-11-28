using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Feedback.Query.GetAll;

public class GetAllFeedbacksQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllFeedbacksQuery, ErrorOr<IEnumerable<FeedbackDto>>>
{
    public async Task<ErrorOr<IEnumerable<FeedbackDto>>> Handle(GetAllFeedbacksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var feedbacks = await unitOfWork.Feedback.GetAllAsync();
            
            var feedbackDtos = feedbacks.Value.Adapt<List<FeedbackDto>>();

            return feedbackDtos;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving feedbacks: {ex.Message}");
        }
    }
}