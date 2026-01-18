using ErrorOr;
using MediatR;

namespace BuyTime_Application.Blockchain.Query.GetArbiterAddress;

public record GetArbiterAddressQuery() : IRequest<ErrorOr<string>>;