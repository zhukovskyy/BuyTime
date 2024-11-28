using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Feedback.Query.GetById;

public record GetFeedbackByIdQuery(
    Guid Id) : IRequest<ErrorOr<List<FeedbackDto>>>;