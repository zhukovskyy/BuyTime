using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Infrastructure.Repositories;

namespace BuyTime_Infrastructure.Common.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private BuyTimeDbContext _context;
    public IStudentRepository Student { get; private set; }
    public ITeacherRepository Teacher { get; private set; }
    
    public UnitOfWork(BuyTimeDbContext context)
    {
        _context = context;
        Student = new StudentRepository(_context);
        Teacher = new TeacherRepository(_context);
    }
}