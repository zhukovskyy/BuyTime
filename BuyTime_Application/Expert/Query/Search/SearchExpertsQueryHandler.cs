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
        // 1. Отримуємо експертів з репозиторію
        var expertsResult = await unitOfWork.User.SearchExpertsAsync(request.Filter);

        if (expertsResult.IsError)
            return expertsResult.Errors;

        // 2. Мапимо в DTO
        var dtos = expertsResult.Value.Adapt<List<ExpertProfileDto>>();

        // 3. Додаткова логіка: проставляємо IsFavorite
        if (request.Filter.CurrentUserId.HasValue)
        {
            // Оптимізація: завантажуємо всі лайки цього студента одним запитом
            var favoriteIds = await unitOfWork.Favorite
                .GetExpertIdsByStudentIdAsync(request.Filter.CurrentUserId.Value);

            // Перевіряємо кожного експерта, чи є він у списку лайкнутих
            foreach (var dto in dtos)
            {
                if (favoriteIds.Contains(dto.Id))
                {
                    dto.IsFavorite = true;
                }
            }
        }

        return dtos;
    }
}