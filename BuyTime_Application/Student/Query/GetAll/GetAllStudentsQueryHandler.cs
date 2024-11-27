using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Student.Query.GetAll;

public class GetAllStudentsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllStudentsQuery, ErrorOr<IEnumerable<StudentDto>>>
{
    public async Task<ErrorOr<IEnumerable<StudentDto>>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var students = await unitOfWork.Student.GetAllStudentsAsync();
            
            var studentDtos = students.Value.Adapt<List<StudentDto>>();

            return studentDtos;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving students: {ex.Message}");
        }
    }
}