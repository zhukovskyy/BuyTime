using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Expert.Query.GetAll;

public record GetAllExpertsQuery() : IRequest<ErrorOr<IEnumerable<UserDto>>>;