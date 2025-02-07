using ErrorOr;
using MediatR;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface IRepository<T> where T : class
{
    Task<ErrorOr<IEnumerable<T>>> GetAllAsync();
    Task<T> GetByIdAsync(Guid id);
    Task<ErrorOr<T>> GetByFirstAndLastNameAsync(string firstName, string lastName);
    Task<ErrorOr<T>> GetByEmailAsync(string email);
    Task<ErrorOr<Guid>> AddAsync(T entity);
    Task<ErrorOr<Unit>> UpdateAsync(T entity);
}