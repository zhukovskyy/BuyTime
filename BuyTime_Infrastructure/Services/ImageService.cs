using BuyTime_Application.Common.Interfaces.IService;
using ErrorOr;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace BuyTime_Infrastructure.Services;

public class ImageService : IImageService
{
    public async Task<ErrorOr<string>> SaveImageAsync(Stream imageStream, string folderName)
    {
        try
        {
            if (folderName.Contains("..") || Path.IsPathRooted(folderName) || folderName.Contains('/') || folderName.Contains('\\'))
            {
                return Error.Validation("InvalidFolder", "Недопустима назва папки.");
            }
            var allowedFolders = new[] { "avatars", "proofs", "portfolio" };
            if (!allowedFolders.Contains(folderName.ToLower()))
            {
                return Error.Validation("InvalidFolder", "Цільова папка заборонена.");
            }

            var baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "images", folderName);
            if (!Directory.Exists(baseFolder)) Directory.CreateDirectory(baseFolder);

            var fileName = Guid.NewGuid().ToString() + ".webp";
            var physicalPath = Path.Combine(baseFolder, fileName);

            using var image = await SixLabors.ImageSharp.Image.LoadAsync(imageStream);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new SixLabors.ImageSharp.Size(500, 500),
                Mode = ResizeMode.Max
            }));

            await image.SaveAsWebpAsync(physicalPath, new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { Quality = 75 });

            return $"/images/{folderName}/{fileName}";
        }
        catch (Exception ex)
        {
            return Error.Failure("Image.ProcessingFailed", ex.Message);
        }
    }

    public void DeleteImage(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;

        if (relativePath.Contains("..") || Path.IsPathRooted(relativePath)) return;
        if (!relativePath.StartsWith("/images/")) return;

        var path = Path.Combine(Directory.GetCurrentDirectory(), relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(path)) File.Delete(path);
    }
}