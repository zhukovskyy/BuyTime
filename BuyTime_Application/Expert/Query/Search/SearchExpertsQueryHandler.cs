using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Expert.Query.Search;

public class SearchExpertsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SearchExpertsQuery, ErrorOr<PagedResult<ExpertProfileDto>>>
{
    public async Task<ErrorOr<PagedResult<ExpertProfileDto>>> Handle(SearchExpertsQuery request, CancellationToken cancellationToken)
    {
        var expertsResult = await unitOfWork.User.SearchExpertsAsync(request.Filter);

        if (expertsResult.IsError)
            return expertsResult.Errors;

        var (items, totalCount) = expertsResult.Value;

        var dtos = items.Adapt<List<ExpertProfileDto>>();

        if (request.Filter.CurrentUserId.HasValue)
        {
            var favoriteIds = await unitOfWork.Favorite
                .GetExpertIdsByStudentIdAsync(request.Filter.CurrentUserId.Value);

            foreach (var dto in dtos)
            {
                if (favoriteIds.Contains(dto.Id))
                {
                    dto.IsFavorite = true;
                }
            }
        }

        var pagedResult = new PagedResult<ExpertProfileDto>(
            items: dtos,
            totalCount: totalCount,
            pageNumber: request.Filter.PageNumber,
            pageSize: request.Filter.PageSize
        );

        return pagedResult;
    }
}