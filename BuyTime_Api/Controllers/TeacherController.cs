using BuyTime_Application.Teacher.Query.GetAll;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/teacher")]
[ApiController]
public class TeacherController(ISender mediatr) : ApiController
{
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var query = new GetAllTeachersQuery();
            var teachers = await mediatr.Send(query);
            if (teachers.IsError)
                return NoContent();
            return Ok(teachers.Value);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while fetching teachers.");
        }
    }
}