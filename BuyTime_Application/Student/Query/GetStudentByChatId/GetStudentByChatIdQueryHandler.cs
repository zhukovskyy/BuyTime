using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Domain.Entities;
using MediatR;
using ErrorOr;

namespace BuyTime_Application.Student.Query.GetStudentByChatId;

public class GetStudentByChatIdQueryHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<GetStudentByChatIdQuery, ErrorOr<User>>
{
    public async Task<ErrorOr<User>> Handle(GetStudentByChatIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var student = await unitOfWork.Student.GetStudentByChatIdAsync(request.ChatId);
            return student;
        }
        catch (Exception ex)
        {
            return Error.Failure("Error: " + ex.Message);
        }
    }
}