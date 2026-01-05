using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Query.GetByExpertId;

public record GetBookingsByExpertIdQuery(Guid ExpertId) : IRequest<ErrorOr<List<BookingDto>>>;