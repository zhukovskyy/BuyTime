using BuyTime_Application.Transaction.Query.GetUserTransactions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuyTime_Api.Controllers;

[Route("api/transaction")]
[ApiController]
public class TransactionController(ISender mediatr) : ApiController
{
    [HttpGet("get-by-user")]
    public async Task<IActionResult> GetUserTransactions([FromQuery] Guid userId)
    {
        var query = new GetUserTransactionsQuery(userId);
        var result = await mediatr.Send(query);

        if (result.IsError)
            return Problem(result.Errors);

        return Ok(result.Value);
    }
}