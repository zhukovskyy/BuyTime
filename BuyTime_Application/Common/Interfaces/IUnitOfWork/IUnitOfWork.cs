using BuyTime_Application.Common.Interfaces.IRepository;

namespace BuyTime_Application.Common.Interfaces.IUnitOfWork;

public interface IUnitOfWork
{
    IStudentRepository Student { get; }
    ITeacherRepository Teacher { get; }
}