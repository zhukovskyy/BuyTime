using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Common.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private BuyTimeDbContext _context;
    public IUserRepository User { get; private set; }
    public ITimeSlotRepository Timeslot { get; private set; }
    public IBookingRepository Booking { get; private set; }
    public IFeedbackRepository Feedback { get; private set; }
    public IWalletRepository Wallet { get; private set; }

    public UnitOfWork(BuyTimeDbContext context)
    {
        _context = context;
        User = new UserRepository(_context);
        Timeslot = new TimeslotRepository(_context);
        Booking = new BookingRepository(_context);
        Feedback = new FeedbackRepository(_context);
        Wallet = new WalletRepository(_context);
    }
    
    public async Task CommitAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while saving changes to the database.", ex);
        }
    }
}