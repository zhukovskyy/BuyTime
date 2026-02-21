using ErrorOr;

namespace BuyTime_Application.Common.Interfaces.IService;

public interface IImageService
{
    Task<ErrorOr<string>> SaveImageAsync(Stream imageStream, string folderName);
    void DeleteImage(string? relativePath);
}