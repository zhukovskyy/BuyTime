using BuyTime_Application.User.Query.GetAll;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/student")]
[ApiController]
public class StudentController(ISender mediatr, IMapper mapper,
    IConfiguration configuration) : ApiController
{
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var query = new GetAllStudentsQuery();
            var students = await mediatr.Send(query);

            if (students.IsError)
            {
                return NoContent(); 
            }

            return Ok(students.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching students.");
        }
    }

}