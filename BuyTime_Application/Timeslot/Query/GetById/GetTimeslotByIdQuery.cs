using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Timeslot.Query.GetById;

public record GetTimeslotByIdQuery(
    Guid Id) : IRequest<ErrorOr<List<TimeslotDto>>>;