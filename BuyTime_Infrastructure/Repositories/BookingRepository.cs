using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using ErrorOr;
using MediatR;

namespace BuyTime_Infrastructure.Repositories;

public class BookingRepository(BuyTimeDbContext context)
    : Repository<Booking>(context), IBookingRepository
{
    public async Task<ErrorOr<Unit>> UpdateAsync(Booking booking)
    {
        try
        {
            var existingBooking = await context.Bookings.FindAsync(booking.Id);
            if(existingBooking == null)
                return Error.Failure("Bookings not found");
            
            existingBooking.Status = booking.Status;
            existingBooking.Message = booking.Message;
            existingBooking.CreatedAt = booking.CreatedAt;
            
            context.Bookings.Update(existingBooking);
            await context.SaveChangesAsync();
    
            return MediatR.Unit.Value;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }
}