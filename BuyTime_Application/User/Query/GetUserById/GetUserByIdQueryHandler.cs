using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Query.GetById;

public class GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserByIdQuery, ErrorOr<UserProfileDto>>
{
    public async Task<ErrorOr<UserProfileDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await unitOfWork.User.GetUserProfileAsync(request.Id);
    }
}