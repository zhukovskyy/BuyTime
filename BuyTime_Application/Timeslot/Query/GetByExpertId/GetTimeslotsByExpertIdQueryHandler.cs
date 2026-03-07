using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Timeslot.Query.GetByExpertId;

public class GetTimeslotsByExpertIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTimeslotsByExpertIdQuery, ErrorOr<List<TimeslotDto>>>
{
    public async Task<ErrorOr<List<TimeslotDto>>> Handle(GetTimeslotsByExpertIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var timeslots = await unitOfWork.Timeslot.GetByExpertIdAsync(request.ExpertId);

            if (timeslots.IsError)
                return timeslots.Errors;

            var timeslotDtos = timeslots.Value.Adapt<List<TimeslotDto>>();

            return timeslotDtos;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving time slots: {ex.Message}");
        }
    }
}