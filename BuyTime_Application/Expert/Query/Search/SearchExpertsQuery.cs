using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Expert.Query.Search;

public record SearchExpertsQuery(SearchExpertRequest Filter) : IRequest<ErrorOr<PagedResult<ExpertProfileDto>>>;