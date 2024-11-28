using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Query.GetAll;

public record GetAllBookingsQuery()
    : IRequest<ErrorOr<IEnumerable<BookingDto>>>;