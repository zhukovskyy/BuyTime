using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using MediatR;
using ErrorOr;

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

            var studentDto = new StudentDto
            {
                FirstName = user.Value.FirstName,
                LastName = user.Value.LastName,
                Email = user.Value.Email,
                Role = user.Value.Role,
                Feedbacks = new List<FeedbackDto>(), 
                Bookings = new List<BookingDto>(),  
            };

            return new List<StudentDto> { studentDto };
        }
        catch (Exception ex)
        {
            return Error.Failure("Error while retrieving student.");
        }
    }
}