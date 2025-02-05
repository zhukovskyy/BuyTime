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

    public async Task<ErrorOr<User>> GetStudentByChatIdAsync(string chatId)
    {
        try
        {
            var student = await dbSet.FirstOrDefaultAsync(user => user.TelegramChatId == chatId);
            if (student == null)
                return Error.NotFound("Student not found");
            return student;
           
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }
}