using BuyTime_Application.Media.Command.UploadMedia;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/media")]
[Authorize]
[ApiController]
public class MediaController(ISender mediatr) : ApiController
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string folder = "avatars")
    {
        if (file == null || file.Length == 0) return BadRequest(new { Message = "File not selected." });

        using var stream = file.OpenReadStream();
        var command = new UploadMediaCommand(stream, folder);

        var result = await mediatr.Send(command);

        if (result.IsError) return Problem(result.Errors);

        return Ok(new { Url = result.Value });
    }
}