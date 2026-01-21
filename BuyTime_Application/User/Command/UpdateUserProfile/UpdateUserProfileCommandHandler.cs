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

        var socialLinkDtos = request.SocialLinks?
            .Select(s => new SocialLinkDto
            {
                Platform = s.Platform,
                UrlOrHandle = s.UrlOrHandle,
                LogoUrl = null 
            })
            .ToList() ?? new List<SocialLinkDto>();

        var result = await unitOfWork.User.UpdateUserProfileAsync(
            userEntity,
            request.LanguageSkills,
            socialLinkDtos, 
            request.SpecializationNames
        );

        if (result.IsError)
            return result.Errors;

        return result.Value.Adapt<UserDto>();
    }
}