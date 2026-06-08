using FluentValidation;

namespace BuyTime_Application.User.Command.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TelegramChatId).NotEmpty();

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Email)
            .Matches(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$")
            .WithMessage("Некоректний формат електронної пошти.")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}