using ErrorOr;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface IStudentRepository : IRepository<BuyTime_Domain.Entities.User>
{ 
    Task<ErrorOr<IEnumerable<BuyTime_Domain.Entities.User>>> GetAllStudentsAsync();
    Task<ErrorOr<BuyTime_Domain.Entities.User>> GetStudentByChatIdAsync(string chatId);
}