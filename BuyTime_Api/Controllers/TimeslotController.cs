using BuyTime_Application.Timeslot.CreateTimeslot;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/timeslot")]
[ApiController]
public class TimeslotController(ISender mediatr) : ApiController
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateTimeslot([FromBody] CreateTimeslotCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await mediatr.Send(command);

        if (result.IsError)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(CreateTimeslot), new { id = result.Value.TimeslotId }, result.Value);
    }
}