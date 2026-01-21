using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Domain.Entities;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.User.Command.RegisterUser;

public class RegisterUserCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterUserCommand, ErrorOr<UserDto>>
{
    public async Task<ErrorOr<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var userEntity = new BuyTime_Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            ExpertNickname = request.ExpertNickname,
            Email = request.Email,
            TelegramChatId = request.TelegramChatId,
            Description = request.Description,
            AvatarUrl = request.AvatarUrl,
            IsExpert = request.IsExpert 
        };

        var socialLinkDtos = request.SocialLinks?   // може все таки треба було зробити окремий дто для інпута чим оце
            .Select(s => new SocialLinkDto
            {
                Platform = s.Platform,
                UrlOrHandle = s.UrlOrHandle,
                LogoUrl = null 
            })
            .ToList() ?? new List<SocialLinkDto>();

        var result = await unitOfWork.User.RegisterUserAsync(
            userEntity,
            request.LanguageSkills ?? new List<LanguageSkillDto>(),
            socialLinkDtos, 
            request.SpecializationNames ?? new List<string>()
        );

        if (result.IsError)
            return result.Errors;

        return result.Value.Adapt<UserDto>();
    }
}