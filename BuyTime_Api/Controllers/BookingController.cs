using BuyTime_Application.Booking.Command.CancelBooking;
using BuyTime_Application.Booking.Command.ConfirmBooking;
using BuyTime_Application.Booking.Command.CreateBooking;
using BuyTime_Application.Booking.Query.GetAll;
using BuyTime_Application.Booking.Query.GetById;
using BuyTime_Application.Booking.Query.GetByTimeSlotId;
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
    
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelBooking([FromBody] CancelBookingCommand command)
    {
        var result = await mediatr.Send(command);
        if (result.IsError)
            return BadRequest(result.Errors);

        return Ok("Booking cancelled successfully.");
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
    
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var query = new GetAllBookingsQuery();
            var bookings = await mediatr.Send(query);
            if (bookings.IsError)
                return NoContent(); 
            return Ok(bookings.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching bookings.");
        }
    }
    
    [HttpGet("get-by-id")]
    public async Task<IActionResult> GetById([FromQuery] Guid id)
    {
        try
        {
            var query = new GetBookingByIdQuery(id);
            var booking = await mediatr.Send(query);
            if (booking.IsError)
                return NotFound();
            return Ok(booking.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching booking.");
        }
    }

    [HttpGet("get-by-timeslot-id")]
    public async Task<IActionResult> GetByTimeSlotId([FromQuery] Guid timeSlotId)
    {
        try
        {
            var query = new GetBookingsByTimeSlotIdQuery(timeSlotId);
            var bookings = await mediatr.Send(query);
            if (bookings.IsError)
                return Problem(bookings.Errors);
            return Ok(bookings.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching bookings.");
        }
    }
}