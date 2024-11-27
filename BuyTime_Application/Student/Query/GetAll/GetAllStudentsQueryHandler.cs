using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
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

            var studentDtos = students.Value.Select(student => new StudentDto
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                Role = student.Role,
                Feedbacks = student.Feedbacks?.Select(fb => new FeedbackDto
                {
                    Rating = fb.Rating,
                    Comment = fb.Comment,
                    CreatedAt = fb.CreatedAt
                }).ToList() ?? new List<FeedbackDto>()
            }).ToList();

            return studentDtos;
        }
        catch (Exception ex)
        {
            return Error.Failure($"Error while retrieving students: {ex.Message}");
        }
    }
}