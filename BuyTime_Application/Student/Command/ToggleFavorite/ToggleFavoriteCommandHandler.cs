using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Student.Command.ToggleFavorite;

public class ToggleFavoriteCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ToggleFavoriteCommand, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(ToggleFavoriteCommand request, CancellationToken cancellationToken)
    {
        // Перевіряємо, чи існує такий запис
        var existingFavorite = await unitOfWork.Favorite.GetAsync(request.StudentId, request.ExpertId);

        if (existingFavorite != null)
        {
            // Якщо є - видаляємо
            await unitOfWork.Favorite.DeleteAsync(existingFavorite);
            await unitOfWork.CommitAsync();
            return false; // false = видалено (зірочка пуста)
        }
        else
        {
            // Якщо немає - додаємо
            var newFavorite = new BuyTime_Domain.Entities.FavoriteExpert
            {
                StudentId = request.StudentId,
                ExpertId = request.ExpertId
            };
            await unitOfWork.Favorite.AddAsync(newFavorite);
            await unitOfWork.CommitAsync();
            return true; // true = додано (зірочка жовта)
        }
    }
}