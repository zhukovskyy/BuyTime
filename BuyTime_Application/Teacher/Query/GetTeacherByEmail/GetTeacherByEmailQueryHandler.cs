using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Application.Student.Query.GetStudentByEmail;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Teacher.Query.GetTeacherByEmail;

public class GetTeacherByEmailQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTeacherByEmailQuery, ErrorOr<List<TeacherDto>>>
{
    public async Task<ErrorOr<List<TeacherDto>>> Handle(GetTeacherByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var teacher = await unitOfWork.Teacher.GetByEmailAsync(request.Email);

            if (teacher.IsError) 
            {
                return Error.Failure("Teacher not found.");
            }

            var teacherDto = new TeacherDto()
            {
                FirstName = teacher.Value.FirstName,
                LastName = teacher.Value.LastName,
                Email = teacher.Value.Email,
                Role = teacher.Value.Role,
                Rating = teacher.Value.Rating,
                Description = teacher.Value.Description,
                Tags = teacher.Value.Tags,
                Timeslots = new List<TimeslotDto>(), 
            };

            return new List<TeacherDto> { teacherDto };
        }
        catch (Exception ex)
        {
            return Error.Failure("Error while retrieving teacher.");
        }
    }
}