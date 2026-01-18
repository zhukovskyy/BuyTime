using BuyTime_Application.Common.Interfaces.IService;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Blockchain.Query.GetArbiterAddress;

public class GetArbiterAddressQueryHandler(IBlockchainService blockchainService)
    : IRequestHandler<GetArbiterAddressQuery, ErrorOr<string>>
{
    public async Task<ErrorOr<string>> Handle(GetArbiterAddressQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var address = await blockchainService.GetArbiterAddressAsync();
            return address;
        }
        catch (Exception ex)
        {
            return Error.Failure("ConfigurationError", ex.Message);
        }
    }
}