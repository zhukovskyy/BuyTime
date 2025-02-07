using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Teacher.Query.GetAll;

public class GetAllTeachersQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllTeachersQuery, ErrorOr<IEnumerable<UserDto>>>
{
    public async Task<ErrorOr<IEnumerable<UserDto>>> Handle(GetAllTeachersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var teachers = await unitOfWork.User.GetAllTeachersAsync();
            
            var teacherDtos = teachers.Value.Adapt<List<UserDto>>();

            return teacherDtos;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving teachers: {ex.Message}");
        }
    }
}