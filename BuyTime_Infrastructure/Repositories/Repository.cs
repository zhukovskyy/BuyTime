using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Infrastructure.Common.Persistence;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Repositories;

public class Repository<T> : IRepository<T>
    where T : class
{
    private readonly BuyTimeDbContext _context;
    internal DbSet<T> dbSet;

    public Repository(BuyTimeDbContext context)
    {
        _context = context;
        this.dbSet = _context.Set<T>();
    }
    
    public async Task<ErrorOr<IEnumerable<T>>> GetAllAsync()
    {
        try
        {
            var entities = await dbSet.ToListAsync();
            return entities;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<T>> GetByFirstAndLastNameAsync(string firstName, string lastName)
    {
        try
        {
            var entity = await dbSet
                .Where(e => EF.Property<string>(e, "FirstName") == firstName &&
                            EF.Property<string>(e, "LastName") == lastName)
                .FirstOrDefaultAsync();
            if (entity == null)
                return Error.Failure("Entity not found");
            return entity;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<T>> GetByEmailAsync(string email)
    {
        try
        {
            var entity = await dbSet.Where(e => EF.Property<string>(e, "Email") == email).FirstOrDefaultAsync();
            if (entity == null)
                return Error.Failure("Entity not found");
            return entity;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }
}