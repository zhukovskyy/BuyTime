using ErrorOr;
using MediatR;

namespace BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;

public interface IBookingRepository : IRepository<BuyTime_Domain.Entities.Booking>
{
    Task<ErrorOr<Unit>> UpdateAsync(BuyTime_Domain.Entities.Booking booking);
    Task<ErrorOr<List<BuyTime_Domain.Entities.Booking>>> GetBookingsByTimeSlotIdAsync(Guid timeSlotId);
    Task<ErrorOr<List<Booking>>> GetBookingsByExpertIdAsync(Guid expertId);
    Task<ErrorOr<List<BuyTime_Domain.Entities.Booking>>> GetBookingsByStudentIdAsync(Guid studentId);
}