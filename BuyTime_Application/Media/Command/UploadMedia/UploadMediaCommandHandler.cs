using BuyTime_Application.Common.Interfaces.IService;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Media.Command.UploadMedia;

public class UploadMediaCommandHandler(IImageService imageService)
    : IRequestHandler<UploadMediaCommand, ErrorOr<string>>
{
    public async Task<ErrorOr<string>> Handle(UploadMediaCommand request, CancellationToken cancellationToken)
    {
        return await imageService.SaveImageAsync(request.FileStream, request.FolderName);
    }
}