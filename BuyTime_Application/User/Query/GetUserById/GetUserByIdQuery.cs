using MediatR;
using ErrorOr;
using BuyTime_Application.Dto;

namespace BuyTime_Application.User.Query.GetById;

public record GetUserByIdQuery(Guid Id) : IRequest<ErrorOr<BuyTime_Domain.Entities.User>>;