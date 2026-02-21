using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Domain.Entities;
using ErrorOr;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Application.User.Command.UpdateUserProfile;

public class UpdateUserProfileCommandHandler(
    IUnitOfWork unitOfWork,
    IImageService imageService
) : IRequestHandler<UpdateUserProfileCommand, ErrorOr<UserDto>>
{
    public async Task<ErrorOr<UserDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await unitOfWork.User.GetByIdAsync(request.UserId);
        var oldAvatarUrl = existingUser?.AvatarUrl;

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

        if (!string.IsNullOrEmpty(oldAvatarUrl) && oldAvatarUrl != request.AvatarUrl)
        {
            imageService.DeleteImage(oldAvatarUrl);
        }

        return result.Value.Adapt<UserDto>();
    }
}