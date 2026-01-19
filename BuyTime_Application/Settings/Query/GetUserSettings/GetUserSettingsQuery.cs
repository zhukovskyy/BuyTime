using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Settings.Query.GetUserSettings;

public record GetUserSettingsQuery(Guid UserId) : IRequest<ErrorOr<UserSettingsDto>>;