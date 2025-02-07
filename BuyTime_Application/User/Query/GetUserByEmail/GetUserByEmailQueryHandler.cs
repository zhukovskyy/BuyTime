using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.User.Query.GetUserByEmail;

public class GetUserByEmailQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserByEmailQuery, ErrorOr<List<UserDto>>>
{
    public async Task<ErrorOr<List<UserDto>>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await unitOfWork.User.GetByEmailAsync(request.Email);

            if (users.IsError) 
            {
                return Error.Failure("User not found.");
            }
            
            var userDto = users.Value.Adapt<UserDto>();

            userDto.Feedbacks = new List<FeedbackDto>();
            userDto.Bookings = new List<BookingDto>();

            return new List<UserDto> { userDto };
        }
        catch (Exception ex)
        {
            return Error.Failure("Error while retrieving user.");
        }
    }
}