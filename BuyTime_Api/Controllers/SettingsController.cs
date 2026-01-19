using BuyTime_Application.Settings.Command.UpdateUserSettings;
using BuyTime_Application.Settings.Query.GetUserSettings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/settings")]
[ApiController]
public class SettingsController(ISender mediatr) : ApiController
{
    [HttpGet("get")]
    public async Task<IActionResult> GetSettings([FromQuery] Guid userId)
    {
        var query = new GetUserSettingsQuery(userId);
        var result = await mediatr.Send(query);

        if (result.IsError)
            return Problem(result.Errors);

        return Ok(result.Value);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateUserSettingsCommand command)
    {
        var result = await mediatr.Send(command);

        if (result.IsError)
            return Problem(result.Errors);

        return Ok(result.Value);
    }
}