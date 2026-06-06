using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Application.User.Query.GetById;
using ErrorOr;
using FluentAssertions;
using Moq;
using Xunit;

namespace BuyTime_Application.UnitTests.User.Query.GetById;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new GetUserByIdQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserProfile_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        var expectedProfile = new UserProfileDto
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            IsExpert = true
        };

        _unitOfWorkMock
            .Setup(u => u.User.GetUserProfileAsync(userId))
            .ReturnsAsync(expectedProfile);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedProfile);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        var expectedError = Error.NotFound("User.NotFound", "Користувача не знайдено");

        _unitOfWorkMock
            .Setup(u => u.User.GetUserProfileAsync(userId))
            .ReturnsAsync(expectedError);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}