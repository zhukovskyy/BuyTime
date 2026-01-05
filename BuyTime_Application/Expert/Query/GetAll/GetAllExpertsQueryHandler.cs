using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Application.Expert.Query.GetAll;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Expert.Query.GetAll;

public class GetAllExpertsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllExpertsQuery, ErrorOr<IEnumerable<UserDto>>>
{
    public async Task<ErrorOr<IEnumerable<UserDto>>> Handle(GetAllExpertsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var experts = await unitOfWork.User.GetAllExpertsAsync();
            var expertDtos = experts.Value.Adapt<List<UserDto>>();
            return expertDtos;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving experts: {ex.Message}");
        }
    }
}