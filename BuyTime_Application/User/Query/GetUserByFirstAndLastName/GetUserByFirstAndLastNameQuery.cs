using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Query.GetUserByFirstAndLastName;

public record GetUserByFirstAndLastNameQuery(
    string FirstName, 
    string LastName) : IRequest<ErrorOr<List<UserDto>>>;