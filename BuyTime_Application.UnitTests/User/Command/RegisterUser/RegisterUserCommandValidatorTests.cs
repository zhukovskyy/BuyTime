using BuyTime_Application.User.Command.RegisterUser;
using FluentAssertions;
using Xunit;

namespace BuyTime_Application.UnitTests.User.Command.RegisterUser;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Theory]

    [InlineData("", "Шевченко", "12345", false)] // Пусте ім'я -> помилка
    [InlineData("Іван", "", "12345", false)] // Пусте прізвище -> помилка
    [InlineData("Іван", "Шевченко", "", false)] // Пустий Telegram -> помилка
    [InlineData("Іван", "Шевченко", "12345", true)] // Коректні дані -> успіх

    public void Validate_RegistrationData_ShouldReturnExpectedResult(
        string firstName, string lastName, string telegramChatId, bool expectedIsValid)
    {
        var command = new RegisterUserCommand(
            FirstName: firstName,
            LastName: lastName,
            ExpertNickname: null,
            Email: "test@example.com",
            TelegramChatId: telegramChatId,
            DiscordId: null,
            Description: null,
            AvatarUrl: null,
            IsExpert: false,
            LanguageSkills: null,
            SocialLinks: null,
            SpecializationNames: null
        );

        var validationResult = _validator.Validate(command);

        validationResult.IsValid.Should().Be(expectedIsValid);
    }

    [Theory]
    [InlineData("invalid-email", false)]
    [InlineData("test@.com", false)]
    [InlineData("valid@example.com", true)]
    [InlineData(null, true)]
    public void Validate_Email_ShouldReturnExpectedResult(string email, bool expectedIsValid)
    {
        var command = new RegisterUserCommand(
            FirstName: "Іван", LastName: "Шевченко", TelegramChatId: "123",
            ExpertNickname: null, Email: email, DiscordId: null, Description: null,
            AvatarUrl: null, IsExpert: false, LanguageSkills: null, SocialLinks: null, SpecializationNames: null
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().Be(expectedIsValid);
    }
}