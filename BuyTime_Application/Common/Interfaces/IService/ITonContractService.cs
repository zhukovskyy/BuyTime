using BuyTime_Application.Dto;
using ErrorOr;

namespace BuyTime_Application.Common.Interfaces.IService;

public interface ITonContractService
{
    Task<ErrorOr<TonConnectPayloadDto>> GenerateCreateBookingPayloadAsync(
        string studentWalletAddress, 
        string expertWalletAddress, 
        DateTime startTime, 
        DateTime endTime, 
        decimal priceAmount);

    Task<ErrorOr<string>> ResolveBookingByArbiterAsync(
        string contractAddress, 
        bool isExpertPresent);

    Task<ErrorOr<TonConnectPayloadDto>> GenerateCancelBookingPayloadAsync(
        bool isStudent, 
        string contractAddress);
}