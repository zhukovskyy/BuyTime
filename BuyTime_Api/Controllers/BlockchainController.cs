using BuyTime_Application.Blockchain.Query.GetArbiterAddress;
using BuyTime_Application.Blockchain.Query.GetPlatformAddress;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/blockchain")]
[ApiController]
public class BlockchainController(ISender mediatr) : ApiController
{
    //[HttpGet("platform-address")]
    //public async Task<IActionResult> GetPlatformAddress()
    //{
    //    var query = new GetPlatformAddressQuery();
    //    var result = await mediatr.Send(query);

    //    if (result.IsError)
    //        return Problem(result.Errors);

    //    return Ok(new { address = result.Value });
    //}

    //[HttpGet("arbiter-address")]
    //public async Task<IActionResult> GetArbiterAddress()
    //{
    //    var query = new GetArbiterAddressQuery();
    //    var result = await mediatr.Send(query);

    //    if (result.IsError)
    //        return Problem(result.Errors);

    //    return Ok(new { address = result.Value });
    //}
}