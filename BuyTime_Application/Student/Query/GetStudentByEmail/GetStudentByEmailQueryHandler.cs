using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.Student.Query.GetStudentByEmail;

public class GetStudentByEmailQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetStudentByEmailQuery, ErrorOr<List<StudentDto>>>
{
    public async Task<ErrorOr<List<StudentDto>>> Handle(GetStudentByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var students = await unitOfWork.Student.GetByEmailAsync(request.Email);

            if (students.IsError) 
            {
                return Error.Failure("Student not found.");
            }
            
            var studentDto = students.Value.Adapt<StudentDto>();

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