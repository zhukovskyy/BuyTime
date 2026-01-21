using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Domain.Entities;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.User.Command.UpdateUserProfile;

public class UpdateUserProfileCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUserProfileCommand, ErrorOr<UserDto>>
{
    public async Task<ErrorOr<UserDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userEntity = new BuyTime_Domain.Entities.User
        {
            Id = request.UserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            ExpertNickname = request.ExpertNickname,
            Email = request.Email,
            Description = request.Description,
            AvatarUrl = request.AvatarUrl
        };

        var languageEntities = request.LanguageSkills.Select(l => new BuyTime_Domain.Entities.LanguageSkill
        {
            Id = Guid.NewGuid(),
            LanguageName = l.LanguageName,
            Level = l.Level
        }).ToList();

        var result = await unitOfWork.User.UpdateUserProfileAsync(
            userEntity,
            languageEntities,
            request.SocialLinks,
            request.SpecializationNames 
        );

        if (result.IsError)
            return result.Errors;

        return result.Value.Adapt<UserDto>();
    }
}