using ErrorOr;
using MediatR;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface IBookingRepository : IRepository<BuyTime_Domain.Entities.Booking>
{
    Task<ErrorOr<Unit>> UpdateAsync(BuyTime_Domain.Entities.Booking booking);
}