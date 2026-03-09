using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Query.GetByStudentId;

public record GetBookingsByStudentIdQuery(Guid StudentId) : IRequest<ErrorOr<List<StudentBookingSummaryDto>>>;