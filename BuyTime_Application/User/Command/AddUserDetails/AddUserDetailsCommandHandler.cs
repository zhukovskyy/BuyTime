using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using Mapster;
using MediatR;

namespace BuyTime_Application.User.Command.AddUserDetails;

public class AddUserDetailsCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<AddUserDetailsCommand, ErrorOr<AddUserDetailsDto>>
{
    public async Task<ErrorOr<AddUserDetailsDto>> Handle(AddUserDetailsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = request.Adapt<BuyTime_Domain.Entities.User>();

            var result = await unitOfWork.User.AddUserDetailsAsync(user);
            if (result.IsError)
                return Error.Failure(result.Errors.ToString() ?? string.Empty);

            return result.Value.Adapt<AddUserDetailsDto>();
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }
}