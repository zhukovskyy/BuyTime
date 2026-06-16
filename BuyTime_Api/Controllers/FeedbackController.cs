using BuyTime_Application.Feedback.Command.Create;
using BuyTime_Application.Feedback.Query.GetAll;
using BuyTime_Application.Feedback.Query.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/feedback")]
[ApiController]
public class FeedbackController(ISender mediatr) : ApiController
{
    public record CreateFeedbackRequest(Guid ExpertId, decimal Rating, string? Comment);

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new CreateFeedbackCommand(CurrentUserId, request.ExpertId, request.Rating, request.Comment);

        var result = await mediatr.Send(command);

        if (result.IsError)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(CreateFeedback), new { id = result.Value.FeedbackId }, result.Value);
    }

    //[HttpGet("get-all")]
    //public async Task<IActionResult> GetAll()
    //{
    //    try
    //    {
    //        var query = new GetAllFeedbacksQuery();
    //        var feedbacks = await mediatr.Send(query);
    //        if (feedbacks.IsError)
    //            return NoContent(); 
    //        return Ok(feedbacks.Value);
    //    }
    //    catch (Exception)
    //    {
    //        return StatusCode(500, "An error occurred while fetching feedbacks.");
    //    }
    //}

    [HttpGet("get-by-id")]
    public async Task<IActionResult> GetById([FromQuery] Guid id)
    {
        try
        {
            var query = new GetFeedbackByIdQuery(id);
            var feedback = await mediatr.Send(query);
            if (feedback.IsError)
                return NotFound();
            return Ok(feedback.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching feedback.");
        }
    }
}