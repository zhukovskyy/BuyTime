using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Query.GetUserByChatId;

public class GetUserByChatIdQueryHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<GetUserByChatIdQuery, ErrorOr<BuyTime_Domain.Entities.User>>
{
    public async Task<ErrorOr<BuyTime_Domain.Entities.User>> Handle(GetUserByChatIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await unitOfWork.User.GetUserByChatIdAsync(request.ChatId);
            return user;
        }
        catch (Exception ex)
        {
            return Error.Failure("Error: " + ex.Message);
        }
    }
}