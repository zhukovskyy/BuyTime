using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Timeslot.Query.GetByExpertId;

public record GetTimeslotsByExpertIdQuery(Guid ExpertId) : IRequest<ErrorOr<List<TimeslotDto>>>;