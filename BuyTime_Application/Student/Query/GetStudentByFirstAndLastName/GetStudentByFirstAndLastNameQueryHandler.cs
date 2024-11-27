using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using MediatR;
using ErrorOr;
using Mapster;

namespace BuyTime_Application.Student.Query.GetStudentByFirstAndLastName;

public class GetStudentByFirstAndLastNameQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetStudentByFirstAndLastNameQuery, ErrorOr<List<StudentDto>>>
{
    public async Task<ErrorOr<List<StudentDto>>> Handle(GetStudentByFirstAndLastNameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await unitOfWork.Student.GetByFirstAndLastNameAsync(request.FirstName, request.LastName);

            if (user.IsError) 
            {
                return Error.Failure("Student not found.");
            }
            
            var studentDto = user.Value.Adapt<StudentDto>();

            studentDto.Feedbacks = new List<FeedbackDto>();
            studentDto.Bookings = new List<BookingDto>();

            return new List<StudentDto> { studentDto };
        }
        catch (Exception ex)
        {
            return Error.Failure("Error while retrieving student.");
        }
    }
}