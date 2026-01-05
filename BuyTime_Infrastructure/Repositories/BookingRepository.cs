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
            if (existingBooking == null)
                return Error.Failure("Bookings not found");

            existingBooking.Status = booking.Status;
            existingBooking.MessageToExpert = booking.MessageToExpert;
            existingBooking.CreatedAt = booking.CreatedAt;
            existingBooking.ConfirmationMessage = booking.ConfirmationMessage;
            existingBooking.MeetingLink = booking.MeetingLink;

            if (booking.Cancellation != null)
            {
                var existingCancellation = await context.BookingCancellations
                    .FirstOrDefaultAsync(bc => bc.BookingId == booking.Id);

                if (existingCancellation == null)
                {
                    await context.BookingCancellations.AddAsync(booking.Cancellation);
                }
            }

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
                .Include(b => b.Cancellation)
                .ToListAsync();

            return bookings;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving bookings: {ex.Message}");
        }
    }

    public async Task<ErrorOr<List<Booking>>> GetBookingsByExpertIdAsync(Guid expertId)
    {
        try
        {
            var bookings = await dbSet
                .Include(b => b.TimeSlot)        
                .ThenInclude(t => t.Expert)     
                .Include(b => b.Student)         
                .Include(b => b.Cancellation)    
                .Where(b => b.TimeSlot.ExpertId == expertId) 
                .OrderByDescending(b => b.TimeSlot.StartTime)
                .ToListAsync();

            return bookings;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving expert bookings: {ex.Message}");
        }
    }

    public override async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await dbSet
            .Include(b => b.Cancellation) 
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}
