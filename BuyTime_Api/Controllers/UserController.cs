using BuyTime_Application.User.Command;
using BuyTime_Application.User.Command.AddUserDetails;
using BuyTime_Application.User.Command.ToggleTeacher;
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

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var query = new GetAllUsersQuery();
            var users = await mediatr.Send(query);
            if (users.IsError)
                return NoContent(); 
            return Ok(users.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching users.");
        }
    }
    
    [HttpGet("get-by-first-and-last-name")]
    public async Task<IActionResult> GetByFirstAndLastName([FromQuery] string firstName, [FromQuery] string lastName)
    {
        try
        {
            var query = new GetUserByFirstAndLastNameQuery(firstName, lastName);
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
    
    [HttpGet("get-by-email")]
    public async Task<IActionResult> GetByEmail([FromQuery] string email)
    {
        try
        {
            var query = new GetUserByEmailQuery(email);
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
    
    [HttpPut("toggle-is-teacher")]
    public async Task<IActionResult> ToggleIsTeacher([FromQuery] Guid userId)
    {
        try
        {
            var command = new ToggleIsTeacherCommand(userId);
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
    
    [HttpPost("add-user-details")]
    public async Task<IActionResult> AddUserDetails([FromBody] AddUserDetailsCommand command)
    {
        try
        {
            var result = await mediatr.Send(command);
            if (result.IsError)
                return StatusCode(409, result.IsError);
            return Ok(result.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while adding user details.");
        }
    }
}