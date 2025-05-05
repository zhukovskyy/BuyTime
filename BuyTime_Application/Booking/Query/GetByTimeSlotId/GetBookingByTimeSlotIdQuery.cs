using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Query.GetByTimeSlotId;

public record GetBookingsByTimeSlotIdQuery(Guid TimeSlotId) : IRequest<ErrorOr<List<BookingDto>>>;