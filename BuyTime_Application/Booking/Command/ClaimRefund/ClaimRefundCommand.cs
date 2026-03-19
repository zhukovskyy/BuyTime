using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Command.ClaimRefund;

public record ClaimRefundCommand(Guid BookingId, Guid StudentId) : IRequest<ErrorOr<TonConnectPayloadDto>>;