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

        var languageEntities = request.LanguageSkills.Select(l => new LanguageSkill
        {
            LanguageName = l.LanguageName,
            Level = l.Level
        }).ToList();

        var result = await unitOfWork.User.RegisterUserAsync(
            userEntity,
            languageEntities,
            request.SocialLinks ?? new List<SocialLinkDto>(),
            request.SpecializationNames ?? new List<string>()
        );

        if (result.IsError)
            return result.Errors;

        return result.Value.Adapt<UserDto>();
    }
}