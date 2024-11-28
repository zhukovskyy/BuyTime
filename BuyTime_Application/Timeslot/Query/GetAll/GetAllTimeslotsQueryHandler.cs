using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Timeslot.Query.GetAll;

public class GetAllTimeslotsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllTimeslotsQuery, ErrorOr<IEnumerable<TimeslotDto>>>
{
    public async Task<ErrorOr<IEnumerable<TimeslotDto>>> Handle(GetAllTimeslotsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var timeslots = await unitOfWork.Timeslot.GetAllAsync();
            
            var timeslotDtos = timeslots.Value.Adapt<List<TimeslotDto>>();

            return timeslotDtos;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving time slots: {ex.Message}");
        }
    }
}