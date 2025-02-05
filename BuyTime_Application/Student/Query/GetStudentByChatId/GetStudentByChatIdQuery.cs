using BuyTime_Domain.Entities;
using MediatR;
using ErrorOr;

namespace BuyTime_Application.Student.Query.GetStudentByChatId;

public record GetStudentByChatIdQuery(string ChatId) : IRequest<ErrorOr<User>>;