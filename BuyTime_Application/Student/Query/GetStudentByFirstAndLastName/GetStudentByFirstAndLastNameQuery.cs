using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Student.Query.GetStudentByFirstAndLastName;

public record GetStudentByFirstAndLastNameQuery(
    string FirstName, 
    string LastName) : IRequest<ErrorOr<List<StudentDto>>>;