using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Repositories;

public class UserRepository(BuyTimeDbContext context)
    : Repository<User>(context), IUserRepository
{
    public async Task<ErrorOr<User>> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await dbSet.FirstOrDefaultAsync(user => user.Id == id);
            if (user == null)
                return Error.NotFound("User not found");
            return user;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }
    public async Task<ErrorOr<User>> GetUserByChatIdAsync(string chatId)
    {
        try
        {
            var user = await dbSet.FirstOrDefaultAsync(user => user.TelegramChatId == chatId);
            if (user == null)
                return Error.NotFound("User not found");
            return user;
           
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<IEnumerable<User>>> GetAllStudentsAsync()
    {
        try
        {
            var students = await dbSet.Where(user => user.IsTeacher == false).ToListAsync();
            return students;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<IEnumerable<User>>> GetAllTeachersAsync()
    {
        try
        {
            var teachers = await dbSet.Where(teacher => teacher.IsTeacher == true).ToListAsync();
            return teachers;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<IEnumerable<User>>> GetAllUsersAsync()
    {
        try
        {
            var users = await dbSet.ToListAsync();
            return users;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<User>> AddUserDetailsAsync(User user)
    {
        try
        {
            await dbSet.AddAsync(user);
            await context.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            return Error.Failure("Error while adding user details");
        }
    }
}