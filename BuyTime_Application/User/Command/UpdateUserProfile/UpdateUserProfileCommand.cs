using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Command.UpdateUserProfile;

public record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string ExpertNickname,
    string Email,
    string Description,
    string? AvatarUrl,

    List<LanguageSkillDto> LanguageSkills,
    List<SocialLinkDto> SocialLinks,
    List<string> SpecializationNames
) : IRequest<ErrorOr<UserDto>>;