using ErrorOr;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface IRepository<T> where T : class
{
    Task<ErrorOr<IEnumerable<T>>> GetAllAsync();
    Task<ErrorOr<T>> GetByFirstAndLastNameAsync(string firstName, string lastName);
    Task<ErrorOr<T>> GetByEmailAsync(string email);
}