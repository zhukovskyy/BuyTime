using BuyTime_Application.Booking.Query.GetById;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Feedback.Query.GetById;

public class GetFeedbackByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetFeedbackByIdQuery, ErrorOr<List<FeedbackDto>>>
{
    public async Task<ErrorOr<List<FeedbackDto>>> Handle(GetFeedbackByIdQuery request, CancellationToken cancellationToken)
    {
        try 
        {
            var feedback = await unitOfWork.Feedback.GetByIdAsync(request.Id);
            if (feedback == null) 
            {
                return Error.Failure("Feedback not found."); 
            }
            var feedbackDto = feedback.Adapt<FeedbackDto>();
            return new List<FeedbackDto> { feedbackDto }; 
        } 
        catch (Exception ex)
        {
            return Error.Failure("Error while retrieving feedback."); 
        }
    }
}