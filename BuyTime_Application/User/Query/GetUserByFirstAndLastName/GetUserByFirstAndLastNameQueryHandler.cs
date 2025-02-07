using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.User.Query.GetUserByFirstAndLastName;

public class GetUserByFirstAndLastNameQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserByFirstAndLastNameQuery, ErrorOr<List<UserDto>>>
{
    public async Task<ErrorOr<List<UserDto>>> Handle(GetUserByFirstAndLastNameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await unitOfWork.User.GetByFirstAndLastNameAsync(request.FirstName, request.LastName);

            if (user.IsError) 
            {
                return Error.Failure("User not found.");
            }
            
            var userDto = user.Value.Adapt<UserDto>();

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