using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Command.RegisterUser;

public record SocialLinkInput(string Platform, string UrlOrHandle);
public record RegisterUserCommand(        
    string FirstName,
    string LastName,
    string? ExpertNickname,  
    string? Email,
    string TelegramChatId,   
    string? Description,
    string? AvatarUrl,
    bool IsExpert,           

    List<LanguageSkillDto>? LanguageSkills,
    List<SocialLinkInput>? SocialLinks,
    List<string>? SpecializationNames 
) : IRequest<ErrorOr<UserDto>>;