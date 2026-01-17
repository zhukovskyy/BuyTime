using ErrorOr;
using MediatR;
using BuyTime_Application.Expert.Query.Search;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface IUserRepository : IRepository<BuyTime_Domain.Entities.User>
{
    Task<ErrorOr<BuyTime_Domain.Entities.User>> GetUserByIdAsync(Guid id);
    Task<ErrorOr<BuyTime_Domain.Entities.User>> GetUserByChatIdAsync(string chatId);
    Task<ErrorOr<IEnumerable<BuyTime_Domain.Entities.User>>> GetAllStudentsAsync();

    Task<ErrorOr<IEnumerable<BuyTime_Domain.Entities.User>>> GetAllExpertsAsync();

    Task<ErrorOr<IEnumerable<BuyTime_Domain.Entities.User>>> GetAllUsersAsync();
    Task<ErrorOr<BuyTime_Domain.Entities.User>> AddUserDetailsAsync(BuyTime_Domain.Entities.User user);

    Task<ErrorOr<IEnumerable<BuyTime_Domain.Entities.User>>> SearchExpertsAsync(SearchExpertRequest filter);
}