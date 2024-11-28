using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Timeslot.Query.GetById;

public class GetTimeslotByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTimeslotByIdQuery, ErrorOr<List<TimeslotDto>>>
{
    public async Task<ErrorOr<List<TimeslotDto>>> Handle(GetTimeslotByIdQuery request, CancellationToken cancellationToken)
    {
        try 
        {
            var timeslot = await unitOfWork.Timeslot.GetByIdAsync(request.Id);
            if (timeslot == null) 
            {
                return Error.Failure("Time slot not found."); 
            }
            var timeslotDto = timeslot.Adapt<TimeslotDto>();
            return new List<TimeslotDto> { timeslotDto }; 
        } 
        catch (Exception ex)
        {
            return Error.Failure("Error while retrieving time slot."); 
        }
    }
}