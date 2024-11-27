using BuyTime_Domain.Entities;
using ErrorOr;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface ITeacherRepository : IRepository<BuyTime_Domain.Entities.Teacher>
{ 
    Task<ErrorOr<IEnumerable<BuyTime_Domain.Entities.Teacher>>> GetAllTeachersAsync();
}