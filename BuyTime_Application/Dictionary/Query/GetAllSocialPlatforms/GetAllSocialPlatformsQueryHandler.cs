using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Dictionary.Query.GetAllSocialPlatforms;

public class GetAllSocialPlatformsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllSocialPlatformsQuery, ErrorOr<List<LookupDto>>>
{
    public async Task<ErrorOr<List<LookupDto>>> Handle(GetAllSocialPlatformsQuery request, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.SocialMediaPlatforms.GetAllAsync();

        if (result.IsError) return result.Errors;

        var dtos = result.Value
            .OrderBy(s => s.Name)
            .Select(s => new LookupDto(s.Id, s.Name, s.LogoUrl))
            .ToList();

        return dtos;
    }
}