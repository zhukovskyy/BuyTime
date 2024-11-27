using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Teacher.Query.GetAll;

public class GetAllTeachersQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllTeachersQuery, ErrorOr<IEnumerable<TeacherDto>>>
{
    public async Task<ErrorOr<IEnumerable<TeacherDto>>> Handle(GetAllTeachersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var teachers = await unitOfWork.Teacher.GetAllTeachersAsync();

            var teacherDtos = teachers.Value.Select(teacher => new TeacherDto
            {
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Email = teacher.Email,
                Description = teacher.Description,
                Rating = teacher.Rating,
                Tags = teacher.Tags,
                Role = teacher.Role,
                Timeslots = teacher.TimeSlots?.Select(ts => new TimeslotDto
                {
                    StartTime = ts.StartTime,
                    EndTime = ts.EndTime,
                    IsAvailable = ts.IsAvailable
                }).ToList() ?? new List<TimeslotDto>() 
            }).ToList();


            return teacherDtos;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving users: {ex.Message}");
        }
    }
}