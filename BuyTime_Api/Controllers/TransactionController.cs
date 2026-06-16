using BuyTime_Application.Transaction.Query.GetUserTransactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/transaction")]
[Authorize]
[ApiController]
public class TransactionController(ISender mediatr) : ApiController
{
    [HttpGet("get-by-user")]
    public async Task<IActionResult> GetUserTransactions([FromQuery] string? network)
    {
        var query = new GetUserTransactionsQuery(CurrentUserId, network);
        var result = await mediatr.Send(query);

        if (result.IsError)
            return Problem(result.Errors);

        return Ok(result.Value);
    }
}