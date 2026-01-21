using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Dictionary.Query.GetAllLanguages;
public record GetAllLanguagesQuery() : IRequest<ErrorOr<List<LanguageDto>>>;