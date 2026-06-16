using BuyTime_Application.Expert.Query.GetAll;
using BuyTime_Application.Expert.Query.Search;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/expert")]
[Authorize]
[ApiController]
public class ExpertController(ISender mediatr) : ApiController
{
    public class SearchExpertApiRequest
    {
        public string? SearchQuery { get; set; }
        public string? Language { get; set; }
        public string? Specialization { get; set; }
        public decimal? MinRating { get; set; }
        public decimal? MaxAveragePriceForFilter { get; set; }
        public string? Currency { get; set; }
        public bool OnlyFavorites { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    //[HttpGet("get-all")]
    //public async Task<IActionResult> GetAll()
    //{
    //    try
    //    {
    //        var query = new GetAllExpertsQuery();
    //        var experts = await mediatr.Send(query);
    //        if (experts.IsError)
    //            return NoContent();
    //        return Ok(experts.Value);
    //    }
    //    catch (Exception)
    //    {
    //        return StatusCode(500, "An error occurred while fetching experts.");
    //    }
    //}

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchExpertApiRequest apiRequest)
    {
        try
        {
            var request = new SearchExpertRequest
            {
                SearchQuery = apiRequest.SearchQuery,
                Language = apiRequest.Language,
                Specialization = apiRequest.Specialization,
                MinRating = apiRequest.MinRating,
                MaxAveragePriceForFilter = apiRequest.MaxAveragePriceForFilter,
                Currency = apiRequest.Currency,
                OnlyFavorites = apiRequest.OnlyFavorites,
                PageNumber = apiRequest.PageNumber,
                PageSize = apiRequest.PageSize,
                CurrentUserId = CurrentUserId
            };

            var query = new SearchExpertsQuery(request);
            var result = await mediatr.Send(query);

            if (result.IsError)
                return Problem(result.Errors);

            return Ok(result.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while searching experts.");
        }
    }
}