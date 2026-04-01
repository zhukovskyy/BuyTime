using BuyTime_Application.Dto;
using BuyTime_Application.User.Command.RegisterUser;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Command.UpdateUserProfile;

public record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? ExpertNickname,
    string? Email,
    string? DiscordId,
    string? Description,
    string? AvatarUrl,

    List<LanguageSkillDto> LanguageSkills,
    List<SocialLinkInput> SocialLinks, 
    List<string> SpecializationNames
) : IRequest<ErrorOr<UserDto>>;