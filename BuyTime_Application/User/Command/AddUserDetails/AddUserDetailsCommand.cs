using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Command.AddUserDetails;

public record AddUserDetailsCommand(
    string FirstName,
    string LastName,
    string Email,
    string TelegramChatId,
    bool IsTeacher,
    string? Description,
    decimal? Rating,
    string? Tags) : IRequest<ErrorOr<AddUserDetailsDto>>;