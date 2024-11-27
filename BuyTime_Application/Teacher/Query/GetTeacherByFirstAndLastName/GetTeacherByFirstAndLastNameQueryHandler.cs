using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
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