using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Query.GetUserByEmail;

public record GetUserByEmailQuery(
    string Email) : IRequest<ErrorOr<List<UserDto>>>;