using BuyTime_Application.Timeslot.CreateTimeslot;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Timeslot.Command.CreateTimeslot;

public record CreateTimeslotCommand(
    Guid ExpertId, 
    DateTime StartTime,
    DateTime EndTime,
    decimal Price,       
    string Currency = "TON" // по дефолту тон
) : IRequest<ErrorOr<CreateTimeslotResult>>;