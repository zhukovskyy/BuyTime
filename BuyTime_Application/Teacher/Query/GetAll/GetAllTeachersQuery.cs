using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Teacher.Query.GetAll;

public record GetAllTeachersQuery()
    : IRequest<ErrorOr<IEnumerable<TeacherDto>>>;