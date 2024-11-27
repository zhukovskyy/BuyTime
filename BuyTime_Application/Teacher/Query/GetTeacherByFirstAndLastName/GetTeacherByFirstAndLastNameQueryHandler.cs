using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Teacher.Query.GetTeacherByFirstAndLastName;

public class GetTeacherByFirstAndLastNameQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTeacherByFirstAndLastNameQuery, ErrorOr<List<TeacherDto>>>
{
    public async Task<ErrorOr<List<TeacherDto>>> Handle(GetTeacherByFirstAndLastNameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var teacher = await unitOfWork.Teacher.GetByFirstAndLastNameAsync(request.FirstName, request.LastName);

            if (teacher.IsError) 
            {
                return Error.Failure("Student not found.");
            }

            var teacherDto = new TeacherDto
            {
                FirstName = teacher.Value.FirstName,
                LastName = teacher.Value.LastName,
                Email = teacher.Value.Email,
                Role = teacher.Value.Role,
                Description = teacher.Value.Description,
                Rating = teacher.Value.Rating,
                Tags = teacher.Value.Tags,
                Timeslots = new List<TimeslotDto>(), 
            };

            return new List<TeacherDto> { teacherDto };
        }
        catch (Exception ex)
        {
            return Error.Failure("Error while retrieving student.");
        }
    }
}