using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using BuyTime_Application.Student.Query.GetStudentByEmail;
using ErrorOr;
using Mapster;
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
            
            var teacherDto = teacher.Value.Adapt<TeacherDto>();

            teacherDto.Timeslots = new List<TimeslotDto>();

            return new List<TeacherDto> { teacherDto };
        }
        catch (Exception ex)
        {
            return Error.Failure("Error while retrieving teacher.");
        }
    }
}