using ErrorOr;
using MediatR;

namespace BuyTime_Application.Media.Command.UploadMedia;

public record UploadMediaCommand(Stream FileStream, string FolderName) : IRequest<ErrorOr<string>>;