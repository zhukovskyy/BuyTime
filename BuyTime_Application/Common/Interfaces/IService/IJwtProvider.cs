namespace BuyTime_Application.Common.Interfaces.IService;

public interface IJwtProvider
{
    string GenerateToken(BuyTime_Domain.Entities.User user);
}