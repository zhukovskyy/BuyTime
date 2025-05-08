using BuyTime_Application.Common.Interfaces.IRepository;

namespace BuyTime_Application.Common.Interfaces.IUnitOfWork;

public interface IUnitOfWork
{
    IUserRepository User { get; }
    ITimeSlotRepository Timeslot { get; }
    IBookingRepository Booking { get; }
    IFeedbackRepository Feedback { get; }
    IWalletRepository Wallet { get; }
    Task CommitAsync();
}