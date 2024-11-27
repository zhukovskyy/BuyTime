using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Student.Query.GetAll;

public record GetAllStudentsQuery()
    : IRequest<ErrorOr<IEnumerable<StudentDto>>>;