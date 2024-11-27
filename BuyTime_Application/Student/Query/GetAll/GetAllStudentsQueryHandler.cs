using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.User.Query.GetAll;

public class GetAllStudentsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllStudentsQuery, ErrorOr<IEnumerable<StudentDto>>>
{
    public async Task<ErrorOr<IEnumerable<StudentDto>>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await unitOfWork.Student.GetAllStudentsAsync();

            var studentDtos = users.Value.Select(user => new StudentDto
            {
                Id = user.Id.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Feedbacks = user.Feedbacks?.Select(fb => new FeedbackDto
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
            return Error.Failure($"Error while retrieving users: {ex.Message}");
        }
    }
}