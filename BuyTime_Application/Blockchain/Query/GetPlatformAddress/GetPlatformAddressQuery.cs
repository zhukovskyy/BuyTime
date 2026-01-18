using ErrorOr;
using MediatR;

namespace BuyTime_Application.Blockchain.Query.GetPlatformAddress;

public record GetPlatformAddressQuery() : IRequest<ErrorOr<string>>;