using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;

namespace BuyTime_Application.Common.Interfaces.IUnitOfWork;

public interface IUnitOfWork
{
    IUserRepository User { get; }
    ITimeSlotRepository Timeslot { get; }
    IBookingRepository Booking { get; }
    IFeedbackRepository Feedback { get; }
    IWalletRepository Wallet { get; }
    IFavoriteExpertRepository Favorite { get; }
    IUserSettingsRepository UserSettings { get; }
    IRepository<Language> Languages { get; }
    IRepository<Specialization> Specializations { get; }
    IRepository<SocialMediaPlatform> SocialMediaPlatforms { get; }
    Task CommitAsync();
}