using ErrorOr;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface IStudentRepository : IRepository<BuyTime_Domain.Entities.User>
{
    public Task<ErrorOr<IEnumerable<BuyTime_Domain.Entities.User>>> GetAllStudentsAsync();
}