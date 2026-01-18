using BuyTime_Domain.Entities;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface IFavoriteExpertRepository
{
    Task<FavoriteExpert?> GetAsync(Guid studentId, Guid expertId);

    // Отримати список ID експертів, яких лайкнув цей студент (для швидкої перевірки IsFavorite)
    Task<HashSet<Guid>> GetExpertIdsByStudentIdAsync(Guid studentId);

    Task AddAsync(FavoriteExpert entity);
    Task DeleteAsync(FavoriteExpert entity);
}