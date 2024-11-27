using BuyTime_Application.Student.Query.GetAll;
using BuyTime_Application.Student.Query.GetStudentByFirstAndLastName;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/student")]
[ApiController]
public class StudentController(ISender mediatr) : ApiController
{
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var query = new GetAllStudentsQuery();
            var students = await mediatr.Send(query);
            if (students.IsError)
                return NoContent(); 
            return Ok(students.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching students.");
        }
    }
    
    [HttpGet("get-by-first-and-last-name")]
    public async Task<IActionResult> GetByFirstAndLastName([FromQuery] string firstName, [FromQuery] string lastName)
    {
        try
        {
            var query = new GetStudentByFirstAndLastNameQuery(firstName, lastName);
            var student = await mediatr.Send(query);
            if (student.IsError)
                return NotFound();
            return Ok(student.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching student.");
        }
    }
}