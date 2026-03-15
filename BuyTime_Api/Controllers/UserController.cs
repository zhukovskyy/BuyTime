using BuyTime_Application.User.Command;
using BuyTime_Application.User.Command.RegisterUser;
using BuyTime_Application.User.Command.ToggleExpert;
using BuyTime_Application.User.Command.UpdateUserProfile;
using BuyTime_Application.User.Query.GetAll;
using BuyTime_Application.User.Query.GetById;
using BuyTime_Application.User.Query.GetUserByChatId;
using BuyTime_Application.User.Query.GetUserByEmail;
using BuyTime_Application.User.Query.GetUserByFirstAndLastName;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(ISender mediatr) : ApiController
{
    [HttpGet("get-by-id")]
    public async Task<IActionResult> GetById([FromQuery] Guid id)
    {
        try
        {
            var query = new GetUserByIdQuery(id);
            var result = await mediatr.Send(query);
            if (result.IsError)
                return NotFound(result.Errors);
            return Ok(result.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching user.");
        }
    }

    //[HttpGet("get-all")]
    //public async Task<IActionResult> GetAll()
    //{
    //    try
    //    {
    //        var query = new GetAllUsersQuery();
    //        var users = await mediatr.Send(query);
    //        if (users.IsError)
    //            return NoContent(); 
    //        return Ok(users.Value);
    //    }
    //    catch (Exception)
    //    {
    //        return StatusCode(500, "An error occurred while fetching users.");
    //    }
    //}

    //[HttpGet("get-by-first-and-last-name")]
    //public async Task<IActionResult> GetByFirstAndLastName([FromQuery] string firstName, [FromQuery] string lastName)
    //{
    //    try
    //    {
    //        var query = new GetUserByFirstAndLastNameQuery(firstName, lastName);
    //        var student = await mediatr.Send(query);
    //        if (student.IsError)
    //            return NotFound();
    //        return Ok(student.Value);
    //    }
    //    catch (Exception)
    //    {
    //        return StatusCode(500, "An error occurred while fetching user.");
    //    }
    //}

    //[HttpGet("get-by-email")]
    //public async Task<IActionResult> GetByEmail([FromQuery] string email)
    //{
    //    try
    //    {
    //        var query = new GetUserByEmailQuery(email);
    //        var student = await mediatr.Send(query);
    //        if (student.IsError)
    //            return NotFound();
    //        return Ok(student.Value);
    //    }
    //    catch (Exception)
    //    {
    //        return StatusCode(500, "An error occurred while fetching user.");
    //    }
    //}

    [HttpGet("get-by-chat-id")]
    public async Task<IActionResult> GetByChatId([FromQuery] string chatId)
    {
        try
        {
            var query = new GetUserByChatIdQuery(chatId);
            var student = await mediatr.Send(query);
            if (student.IsError)
                return NotFound();
            return Ok(student.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching user.");
        }
    }
    
    [HttpPut("toggle-is-expert")]
    public async Task<IActionResult> ToggleIsTeacher([FromQuery] Guid userId)
    {
        try
        {
            var command = new ToggleIsExpertCommand(userId);
            var result = await mediatr.Send(command);
            if (result.IsError)
                return StatusCode(409, result.IsError);
            return Ok();
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while toggling user's role.");
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await mediatr.Send(command);

            return result.Match(
                user => CreatedAtAction(nameof(GetById), new { id = user.Id }, user),
                errors => Problem(errors)
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("update-profile")]

    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await mediatr.Send(command);

            if (result.IsError)
                return Problem(result.Errors);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}