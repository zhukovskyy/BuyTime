using BuyTime_Application.Timeslot.Command.CreateTimeslot;
using BuyTime_Application.Timeslot.CreateTimeslot;
using BuyTime_Application.Timeslot.Query.GetByExpertId;
using BuyTime_Application.Timeslot.Command.UpdateTimeslot;
using BuyTime_Application.Timeslot.Command.DeleteTimeslot;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/timeslot")]
[ApiController]
public class TimeslotController(ISender mediatr) : ApiController
{
    public record CreateTimeslotRequest(DateTime StartTime, DateTime EndTime, decimal Price, string Currency = "TON");
    public record UpdateTimeslotRequest(Guid TimeslotId, DateTime StartTime, DateTime EndTime, decimal Price, string Currency);

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateTimeslot([FromBody] CreateTimeslotRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new CreateTimeslotCommand(CurrentUserId, request.StartTime, request.EndTime, request.Price, request.Currency);
        var result = await mediatr.Send(command);

        if (result.IsError)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(CreateTimeslot), new { id = result.Value.TimeslotId }, result.Value);
    }

    [HttpGet("my-slots")]
    [Authorize]
    public async Task<IActionResult> GetMyTimeslots()
    {
        var query = new GetTimeslotsByExpertIdQuery(CurrentUserId);
        var result = await mediatr.Send(query);

        if (result.IsError)
            return Problem(result.Errors);

        return Ok(result.Value);
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
    [Authorize]
    public async Task<IActionResult> UpdateTimeslot([FromBody] UpdateTimeslotRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new UpdateTimeslotCommand(request.TimeslotId, CurrentUserId, request.StartTime, request.EndTime, request.Price, request.Currency);
        var result = await mediatr.Send(command);

        if (result.IsError)
            return Problem(result.Errors);

        return Ok(new { message = "Таймслот успішно оновлено." });
    }

    [HttpDelete("delete")]
    [Authorize]
    public async Task<IActionResult> DeleteTimeslot([FromQuery] Guid id)
    {
        var command = new DeleteTimeslotCommand(id, CurrentUserId);
        var result = await mediatr.Send(command);

        if (result.IsError)
            return Problem(result.Errors);

        return Ok(new { message = "Таймслот успішно видалено." });
    }
}