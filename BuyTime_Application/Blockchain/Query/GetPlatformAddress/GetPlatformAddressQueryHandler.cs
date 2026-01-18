using BuyTime_Application.Common.Interfaces.IService;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Blockchain.Query.GetPlatformAddress;

public class GetPlatformAddressQueryHandler(IBlockchainService blockchainService)
    : IRequestHandler<GetPlatformAddressQuery, ErrorOr<string>>
{
    public async Task<ErrorOr<string>> Handle(GetPlatformAddressQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var address = await blockchainService.GetPlatformAddressAsync();
            return address;
        }
        catch (Exception ex)
        {
            return Error.Failure("ConfigurationError", ex.Message);
        }
    }
}