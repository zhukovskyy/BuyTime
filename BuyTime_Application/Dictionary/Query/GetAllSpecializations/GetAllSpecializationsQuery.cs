using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Dictionary.Query.GetAllSpecializations;

public record GetAllSpecializationsQuery() : IRequest<ErrorOr<List<LookupDto>>>;