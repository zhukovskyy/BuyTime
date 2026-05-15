using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Common.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private BuyTimeDbContext _context;
    public IUserRepository User { get; private set; }
    public ITimeSlotRepository Timeslot { get; private set; }
    public IBookingRepository Booking { get; private set; }
    public ITransactionRepository Transactions { get; private set; }
    public IFeedbackRepository Feedback { get; private set; }
    public IWalletRepository Wallet { get; private set; }
    public IFavoriteExpertRepository Favorite { get; private set; }
    public IUserSettingsRepository UserSettings { get; private set; }
    public IRepository<Language> Languages { get; private set; }
    public IRepository<Specialization> Specializations { get; private set; }
    public IRepository<SocialMediaPlatform> SocialMediaPlatforms { get; private set; }


    public UnitOfWork(BuyTimeDbContext context)
    {
        _context = context;
        User = new UserRepository(_context);
        Timeslot = new TimeslotRepository(_context);
        Booking = new BookingRepository(_context);
        Transactions = new TransactionRepository(_context);
        Feedback = new FeedbackRepository(_context);
        Wallet = new WalletRepository(_context);
        Favorite = new FavoriteExpertRepository(_context);
        UserSettings = new UserSettingsRepository(_context);
        Languages = new Repository<Language>(_context);
        Specializations = new Repository<Specialization>(_context);
        SocialMediaPlatforms = new Repository<SocialMediaPlatform>(_context);
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