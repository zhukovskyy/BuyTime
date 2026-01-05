using BuyTime_Application.Expert.Query.GetAll;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/expert")]
[ApiController]
public class ExpertController(ISender mediatr) : ApiController
{
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var query = new GetAllExpertsQuery();
            var experts = await mediatr.Send(query);
            if (experts.IsError)
                return NoContent();
            return Ok(experts.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching experts.");
        }
    }
}