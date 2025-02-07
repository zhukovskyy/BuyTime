using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Query.GetAll;

public record GetAllUsersQuery()
    : IRequest<ErrorOr<IEnumerable<UserDto>>>;