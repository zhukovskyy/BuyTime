using BuyTime_Application.Booking.Command.ConfirmBooking;
using BuyTime_Application.Booking.Command.CreateBooking;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/booking")]
[ApiController]
public class BookingController(ISender mediatr) : ApiController
{
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmBooking([FromBody] ConfirmBookingCommand command)
    {
        var result = await mediatr.Send(command);
        if (result.IsError)
            return BadRequest(result.Errors);

        return Ok("Booking confirmed successfully.");
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await mediatr.Send(command);

        if (result.IsError)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(CreateBooking), new { id = result.Value.BookingId }, result.Value);
    }

}