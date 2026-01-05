using BuyTime_Application.Meeting.Command.GenerateZoomLink;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/meeting")]
[ApiController]
public class MeetingController(ISender mediatr) : ApiController
{
    [HttpPost("generate-zoom-link")]
    public async Task<IActionResult> GenerateZoomLink([FromBody] GenerateZoomLinkCommand command)
    {
        var result = await mediatr.Send(command);

        if (result.IsError)
            return Problem(result.Errors);

        return Ok(new { link = result.Value });
    }
}