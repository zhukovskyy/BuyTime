using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Student.Query.GetStudentByEmail;

public class GetStudentByEmailQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetStudentByEmailQuery, ErrorOr<List<StudentDto>>>
{
    public async Task<ErrorOr<List<StudentDto>>> Handle(GetStudentByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await unitOfWork.Student.GetByEmailAsync(request.Email);

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