using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Dictionary.Query.GetAllLanguages;

public class GetAllLanguagesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllLanguagesQuery, ErrorOr<List<LanguageDto>>>
{
    public async Task<ErrorOr<List<LanguageDto>>> Handle(GetAllLanguagesQuery request, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Languages.GetAllAsync();

        if (result.IsError) return result.Errors;

        var dtos = result.Value
            .OrderBy(l => l.Code)
            .Select(l => new LanguageDto(l.Id, l.Code))
            .ToList();

        return dtos;
    }
}