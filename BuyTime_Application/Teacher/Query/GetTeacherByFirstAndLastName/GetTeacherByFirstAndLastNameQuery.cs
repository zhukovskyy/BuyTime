using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Teacher.Query.GetTeacherByFirstAndLastName;

public record GetTeacherByFirstAndLastNameQuery(
    string FirstName, 
    string LastName) : IRequest<ErrorOr<List<TeacherDto>>>;