using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Application.Expert.Query.Search;
using BuyTime_Domain.Entities;
using ErrorOr;
using FluentAssertions;
using Mapster;
using Moq;
using Xunit;

using DomainFeedback = BuyTime_Domain.Entities.Feedback;
using DomainTimeslot = BuyTime_Domain.Entities.Timeslot;
using DomainUser = BuyTime_Domain.Entities.User;

namespace BuyTime_Application.UnitTests.Expert.Query.Search;

public class SearchExpertsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SearchExpertsQueryHandler _handler;

    public SearchExpertsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new SearchExpertsQueryHandler(_unitOfWorkMock.Object);

        TypeAdapterConfig<DomainUser, ExpertProfileDto>.NewConfig()
            .Map(dest => dest.HappyStudentsCount, src => src.ReceivedFeedbacks != null ? src.ReceivedFeedbacks.Count(f => f.Rating >= 4) : 0)
            .Map(dest => dest.ReviewCount, src => src.ReceivedFeedbacks != null ? src.ReceivedFeedbacks.Count : 0)
            .Map(dest => dest.Feedbacks, src => src.ReceivedFeedbacks)
            .Map(dest => dest.Specializations, src => src.Specializations)
            .Map(dest => dest.LanguageSkills, src => src.ExpertLanguages)
            .Map(dest => dest.SocialLinks, src => src.SocialLinks);

        TypeAdapterConfig<BuyTime_Domain.Entities.ExpertSocialLink, SocialLinkDto>.NewConfig()
            .Map(dest => dest.Platform, src => src.Platform.Name)
            .Map(dest => dest.LogoUrl, src => src.Platform.LogoUrl);

        TypeAdapterConfig<BuyTime_Domain.Entities.ExpertLanguage, LanguageSkillDto>.NewConfig()
            .Map(dest => dest.LanguageCode, src => src.Language.Code)
            .Map(dest => dest.Level, src => src.Level);
    }

    [Fact]
    public async Task Handle_ShouldMapAllNestedProperties_WhenRepositoryReturnsExpert()
    {
        var request = new SearchExpertRequest { SearchQuery = "Test", PageNumber = 1, PageSize = 10 };
        var query = new SearchExpertsQuery(request);

        var expertId = Guid.NewGuid();
        var mockExpert = new DomainUser
        {
            Id = expertId,
            IsExpert = true,
            FirstName = "Олег",
            LastName = "Тестовий",

            Specializations = new List<Specialization>
            {
                new Specialization { Id = Guid.NewGuid(), Name = "Backend" },
                new Specialization { Id = Guid.NewGuid(), Name = "C#" }
            },

            ReceivedFeedbacks = new List<DomainFeedback>
            {
                new DomainFeedback { Rating = 5, Comment = "Супер!" },
                new DomainFeedback { Rating = 4, Comment = "Добре" },
                new DomainFeedback { Rating = 2, Comment = "Погано" }
            },

            ExpertLanguages = new List<ExpertLanguage>
            {
                new ExpertLanguage
                {
                    Level = "Native",
                    Language = new Language { Code = "uk" }
                }
            },

            SocialLinks = new List<ExpertSocialLink>
            {
                new ExpertSocialLink
                {
                    UrlOrHandle = "t.me/oleg",
                    Platform = new SocialMediaPlatform { Name = "Telegram", LogoUrl = "url" }
                }
            },

            TimeSlots = new List<DomainTimeslot>()
        };

        var dbResult = new List<DomainUser> { mockExpert };

        _unitOfWorkMock.Setup(u => u.User.SearchExpertsAsync(request))
            .ReturnsAsync((ErrorOr<(IEnumerable<DomainUser> Items, int TotalCount)>)(dbResult, dbResult.Count));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();

        var expertDto = result.Value.Items.First();

        expertDto.Id.Should().Be(expertId);

        expertDto.Specializations.Should().HaveCount(2);
        expertDto.Specializations.Select(s => s.Name).Should().Contain(new[] { "Backend", "C#" });

        expertDto.ReviewCount.Should().Be(3);
        expertDto.HappyStudentsCount.Should().Be(2);
        expertDto.Feedbacks.Should().HaveCount(3);

        expertDto.LanguageSkills.Should().HaveCount(1);
        expertDto.LanguageSkills.First().LanguageCode.Should().Be("uk");

        expertDto.SocialLinks.Should().HaveCount(1);
        expertDto.SocialLinks.First().Platform.Should().Be("Telegram");
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyMatchingExperts_WhenFiltersAreApplied()
    {
        var request = new SearchExpertRequest
        {
            Language = "en",
            MinRating = 4.5m,
            PageNumber = 1,
            PageSize = 10
        };
        var query = new SearchExpertsQuery(request);

        var expert1 = new DomainUser
        {
            Id = Guid.NewGuid(),
            FirstName = "English",
            Rating = 5,
            ExpertLanguages = new List<ExpertLanguage> { new ExpertLanguage { Language = new Language { Code = "en" } } }
        };

        var expert2 = new DomainUser
        {
            Id = Guid.NewGuid(),
            FirstName = "LowRating",
            Rating = 3,
            ExpertLanguages = new List<ExpertLanguage> { new ExpertLanguage { Language = new Language { Code = "en" } } }
        };

        var dbResult = new List<DomainUser> { expert1, expert2 };

        var filteredResult = dbResult.Where(e => e.Rating >= request.MinRating &&
                                                 e.ExpertLanguages.Any(l => l.Language.Code == request.Language))
                                     .ToList();

        _unitOfWorkMock.Setup(u => u.User.SearchExpertsAsync(It.IsAny<SearchExpertRequest>()))
            .ReturnsAsync((ErrorOr<(IEnumerable<DomainUser> Items, int TotalCount)>)(filteredResult, filteredResult.Count));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().FirstName.Should().Be("English");
    }
}