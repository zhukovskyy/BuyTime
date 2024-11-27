using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
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
            
            var teacherDtos = teachers.Value.Adapt<List<TeacherDto>>();

            return teacherDtos;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving teachers: {ex.Message}");
        }
    }
}