using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Query.GetById;

public record GetBookingByIdQuery(
    Guid Id) : IRequest<ErrorOr<List<BookingDto>>>;