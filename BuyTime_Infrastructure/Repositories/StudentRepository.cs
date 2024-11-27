using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Constants;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Repositories;

public class StudentRepository(BuyTimeDbContext context)
    : Repository<User>(context), IStudentRepository
{
    public async Task<ErrorOr<IEnumerable<User>>> GetAllStudentsAsync()
    {
        try
        {
            var students = await dbSet.Where(user => user.Role == Roles.Student)
                .ToListAsync();
            return students;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }
}