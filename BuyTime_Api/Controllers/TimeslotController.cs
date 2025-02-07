using BuyTime_Application.Timeslot.Command.CreateTimeslot;
using BuyTime_Application.Timeslot.CreateTimeslot;
using BuyTime_Application.Timeslot.Query.GetAll;
using BuyTime_Application.Timeslot.Query.GetById;
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
    
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var query = new GetAllTimeslotsQuery();
            var timeslots = await mediatr.Send(query);
            if (timeslots.IsError)
                return NoContent(); 
            return Ok(timeslots.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching timeslots.");
        }
    }
    
    [HttpGet("get-by-id")]
    public async Task<IActionResult> GetById([FromQuery] Guid id)
    {
        try
        {
            var query = new GetTimeslotByIdQuery(id);
            var timeslot = await mediatr.Send(query);
            if (timeslot.IsError)
                return NotFound();
            return Ok(timeslot.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching time slot.");
        }
    }
}