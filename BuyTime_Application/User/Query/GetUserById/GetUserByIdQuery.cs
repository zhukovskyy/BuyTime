using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Query.GetById;

public record GetUserByIdQuery(Guid Id) : IRequest<ErrorOr<UserProfileDto>>;