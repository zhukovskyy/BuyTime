using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Dictionary.Query.GetAllSocialPlatforms;

public record GetAllSocialPlatformsQuery() : IRequest<ErrorOr<List<LookupDto>>>;