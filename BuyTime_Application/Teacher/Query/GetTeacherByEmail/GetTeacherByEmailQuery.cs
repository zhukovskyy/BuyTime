using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Teacher.Query.GetTeacherByEmail;

public record GetTeacherByEmailQuery(
    string Email) : IRequest<ErrorOr<List<TeacherDto>>>;