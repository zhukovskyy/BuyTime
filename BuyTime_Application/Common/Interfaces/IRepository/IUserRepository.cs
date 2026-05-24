using BuyTime_Application.Dto;
using BuyTime_Application.Expert.Query.Search;
using BuyTime_Domain.Entities;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface IUserRepository : IRepository<BuyTime_Domain.Entities.User>
{
    Task<ErrorOr<BuyTime_Domain.Entities.User>> GetUserByIdAsync(Guid id);
    Task<ErrorOr<BuyTime_Domain.Entities.User>> GetUserByChatIdAsync(string chatId);
    Task<ErrorOr<IEnumerable<BuyTime_Domain.Entities.User>>> GetAllStudentsAsync();

    Task<ErrorOr<IEnumerable<BuyTime_Domain.Entities.User>>> GetAllExpertsAsync();

    Task<ErrorOr<IEnumerable<BuyTime_Domain.Entities.User>>> GetAllUsersAsync();
    Task<ErrorOr<BuyTime_Domain.Entities.User>> AddUserDetailsAsync(BuyTime_Domain.Entities.User user);

    Task<ErrorOr<IEnumerable<BuyTime_Domain.Entities.User>>> SearchExpertsAsync(SearchExpertRequest filter);

    Task<ErrorOr<UserProfileDto>> GetUserProfileAsync(Guid id);
    Task<ErrorOr<BuyTime_Domain.Entities.User>> RegisterUserAsync(
        BuyTime_Domain.Entities.User userEntity,
        List<LanguageSkillDto> languageDtos,
        List<SocialLinkDto> socialLinks,
        List<string> specializationNames);

    Task<ErrorOr<BuyTime_Domain.Entities.User>> UpdateUserProfileAsync(
        BuyTime_Domain.Entities.User userChanges,
        List<LanguageSkillDto> languageDtos,
        List<SocialLinkDto> newSocials,
        List<string> newSpecializationNames);
}