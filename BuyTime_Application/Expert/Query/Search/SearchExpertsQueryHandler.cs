using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Expert.Query.Search;

public class SearchExpertsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SearchExpertsQuery, ErrorOr<IEnumerable<ExpertProfileDto>>>
{
    public async Task<ErrorOr<IEnumerable<ExpertProfileDto>>> Handle(SearchExpertsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var experts = await unitOfWork.User.SearchExpertsAsync(request.Filter);

            // Mapster config should handle the basic mapping, 
            // but complex calculations are done in Repository projection or here.
            // In this case, the Repository returns fully populated Entities with includes.

            // Note: Ideally, projection happens in the Repo to save DB bandwidth, 
            // but for simplicity with your existing Adapt setup:
            var dtos = experts.Value.Adapt<List<ExpertProfileDto>>();

            return dtos;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }
}