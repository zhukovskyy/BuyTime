using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Feedback.Query.GetAll;

public record GetAllFeedbacksQuery()
    : IRequest<ErrorOr<IEnumerable<FeedbackDto>>>;