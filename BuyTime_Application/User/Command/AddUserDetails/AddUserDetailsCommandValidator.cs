using FluentValidation;

namespace BuyTime_Application.User.Command.AddUserDetails;

public class AddUserDetailsCommandValidator : AbstractValidator<AddUserDetailsCommand>
{
    public AddUserDetailsCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must be at most 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must be at most 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be at most 500 characters.");

        RuleFor(x => x.Tags)
            .MaximumLength(200).WithMessage("Tags must be at most 200 characters.");
    }
}