using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.User.Query.GetById;
using MediatR;
using ErrorOr;
using BuyTime_Domain.Entities;
using BuyTime_Application.Dto;

namespace BuyTime_Application.User.Query.GetById;

public class GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserByIdQuery, ErrorOr<BuyTime_Domain.Entities.User>>
{
    public async Task<ErrorOr<BuyTime_Domain.Entities.User>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await unitOfWork.User.GetByIdAsync(request.Id);
            return user;
        }
        catch (Exception ex)
        {
            return Error.Failure("Error: " + ex.Message);
        }
    }
}