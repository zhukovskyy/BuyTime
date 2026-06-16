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

            // --- ЗБЕРЕЖЕННЯ МАРКЕРА ---
            if (booking.Attendances != null && booking.Attendances.Any())
            {
                foreach (var attendance in booking.Attendances)
                {
                    var exists = await context.MeetingAttendances.AnyAsync(a => a.Id == attendance.Id);
                    if (!exists)
                    {
                        await context.MeetingAttendances.AddAsync(attendance);
                    }
                }
            }
            // ----------------------------------

            if (booking.Cancellation != null)
            {
                var existingCancellation = await context.BookingCancellations
                    .FirstOrDefaultAsync(bc => bc.BookingId == booking.Id);

                if (existingCancellation == null)
                {
                    await context.BookingCancellations.AddAsync(booking.Cancellation);
                }
            }

            if (booking.RefundRequest != null)
            {
                var existingRefund = await context.RefundRequests
                    .FirstOrDefaultAsync(rr => rr.BookingId == booking.Id);

                if (existingRefund == null)
                {
                    await context.RefundRequests.AddAsync(booking.RefundRequest);
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
                .Include(b => b.TimeSlot)
                .Include(b => b.Cancellation)
                .Include(b => b.RefundRequest)
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
                .Include(b => b.RefundRequest)
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
            .Include(b => b.TimeSlot)
            .Include(b => b.Cancellation)
            .Include(b => b.RefundRequest)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public override async Task<ErrorOr<IEnumerable<Booking>>> GetAllAsync()
    {
        try
        {
            var bookings = await dbSet
                .Include(b => b.TimeSlot) 
                .Include(b => b.Student) 
                .ToListAsync();

            return bookings;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<List<Booking>>> GetBookingsByStudentIdAsync(Guid studentId)
    {
        try
        {
            var bookings = await dbSet
                .Include(b => b.TimeSlot)
                    .ThenInclude(ts => ts.Expert)
                .Include(b => b.Student)
                .Include(b => b.Cancellation)
                .Include(b => b.RefundRequest)
                .Where(b => b.StudentId == studentId)
                .OrderByDescending(b => b.TimeSlot.StartTime)
                .ToListAsync();

            return bookings;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving student bookings: {ex.Message}");
        }
    }

    public async Task<bool> HasCompletedBookingAsync(Guid studentId, Guid expertId)
    {
        return await dbSet.AnyAsync(b =>
            b.StudentId == studentId &&
            b.TimeSlot.ExpertId == expertId &&
            b.Status == BuyTime_Domain.Constants.Status.Completed);
    }
}
