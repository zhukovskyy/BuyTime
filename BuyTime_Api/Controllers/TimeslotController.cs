using BuyTime_Application.Timeslot.Command.CreateTimeslot;
using BuyTime_Application.Timeslot.CreateTimeslot;
using BuyTime_Application.Timeslot.Query.GetByExpertId;
using BuyTime_Application.Timeslot.Command.UpdateTimeslot;
using BuyTime_Application.Timeslot.Command.DeleteTimeslot;
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

    [HttpGet("get-by-expert-id")]
    public async Task<IActionResult> GetByExpertId([FromQuery] Guid expertId)
    {
        try
        {
            var query = new GetTimeslotsByExpertIdQuery(expertId);
            var result = await mediatr.Send(query);

            if (result.IsError)
                return Problem(result.Errors);

            return Ok(result.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching time slots.");
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateTimeslot([FromBody] UpdateTimeslotCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await mediatr.Send(command);

        if (result.IsError)
            return Problem(result.Errors);

        return Ok(new { message = "Таймслот успішно оновлено." });
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteTimeslot([FromQuery] Guid id, [FromQuery] Guid expertId)
    {
        var command = new DeleteTimeslotCommand(id, expertId);
        var result = await mediatr.Send(command);

        if (result.IsError)
            return Problem(result.Errors);

        return Ok(new { message = "Таймслот успішно видалено." });
    }
}