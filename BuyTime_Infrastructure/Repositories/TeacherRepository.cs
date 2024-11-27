using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Constants;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Repositories;

public class TeacherRepository(BuyTimeDbContext context) 
    : Repository<Teacher>(context), ITeacherRepository
{
    public async Task<ErrorOr<IEnumerable<Teacher>>> GetAllTeachersAsync()
    {
        try
        {
            var teachers = await dbSet.Where(t => t.Role == Roles.Teacher)
                .ToListAsync();
            return teachers;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }
}