using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Timeslot.Query.GetAll;

public record GetAllTimeslotsQuery()
    : IRequest<ErrorOr<IEnumerable<TimeslotDto>>>;