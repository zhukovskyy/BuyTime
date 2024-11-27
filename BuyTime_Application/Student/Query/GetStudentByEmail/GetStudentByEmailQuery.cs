using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Student.Query.GetStudentByEmail;

public record GetStudentByEmailQuery(
    string Email) : IRequest<ErrorOr<List<StudentDto>>>;