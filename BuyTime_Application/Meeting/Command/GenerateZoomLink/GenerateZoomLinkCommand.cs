using ErrorOr;
using MediatR;

namespace BuyTime_Application.Meeting.Command.GenerateZoomLink;

public record GenerateZoomLinkCommand(Guid BookingId) : IRequest<ErrorOr<string>>;