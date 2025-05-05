using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
            existingBooking.Answer = booking.Answer;
            existingBooking.UrlOfMeeting = booking.UrlOfMeeting;
            
            context.Bookings.Update(existingBooking);
            await context.SaveChangesAsync();
    
            return MediatR.Unit.Value;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<List<Booking>>> GetBookingsByTimeSlotIdAsync(Guid timeSlotId)
    {
        try
        {
            var bookings = await dbSet
                .Where(b => b.TimeslotId == timeSlotId)
                .ToListAsync();
            return bookings;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving bookings: {ex.Message}");
        }
    }
}