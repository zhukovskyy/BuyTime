using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Dictionary.Query.GetAllSpecializations;

public class GetAllSpecializationsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllSpecializationsQuery, ErrorOr<List<LookupDto>>>
{
    public async Task<ErrorOr<List<LookupDto>>> Handle(GetAllSpecializationsQuery request, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Specializations.GetAllAsync();

        if (result.IsError) return result.Errors;

        var dtos = result.Value
            .OrderBy(s => s.Name)
            .Select(s => new LookupDto(s.Id, s.Name, null))
            .ToList();

        return dtos;
    }
}