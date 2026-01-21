using BuyTime_Api.Controllers;
using BuyTime_Application.Dictionary.Query.GetAllLanguages;
using BuyTime_Application.Dictionary.Query.GetAllSocialPlatforms;
using BuyTime_Application.Dictionary.Query.GetAllSpecializations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/dictionary")]
[ApiController]
public class DictionaryController(ISender mediatr) : ApiController
{
    [HttpGet("specializations")]
    public async Task<IActionResult> GetSpecializations()
    {
        var result = await mediatr.Send(new GetAllSpecializationsQuery());
        return result.Match(Ok, Problem);
    }

    [HttpGet("social-platforms")]
    public async Task<IActionResult> GetSocialPlatforms()
    {
        var result = await mediatr.Send(new GetAllSocialPlatformsQuery());
        return result.Match(Ok, Problem);
    }

    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages()
    {
        var result = await mediatr.Send(new GetAllLanguagesQuery());
        return result.Match(Ok, Problem);
    }
}