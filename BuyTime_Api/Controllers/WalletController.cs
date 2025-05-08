using BuyTime_Application.Wallet.Command.SetWalletByUserIdCommand;
using BuyTime_Application.Wallet.Command.DeleteWalletByUserIdCommand;
using BuyTime_Application.Wallet.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BuyTime_Api.Controllers;

[Route("api/wallet")]
[ApiController]
public class WalletController : ApiController
{
    private readonly ISender _mediator;

    public WalletController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-by-user-id")]
    public async Task<IActionResult> GetWalletByUserId([FromQuery] Guid userId)
    {
        try
        {
            var query = new GetWalletByUserIdQuery(userId);
            var result = await _mediator.Send(query);
            if (result.IsError)
                return Problem(result.Errors);
            return Ok(result.Value);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while fetching the wallet.");
        }
    }

    [HttpPost("set-by-user-id")]
    public async Task<IActionResult> SetWalletByUserId([FromBody] SetWalletByUserIdCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (result.IsError)
            return Problem(result.Errors);

        return Ok("Wallet set successfully.");
    }

    [HttpDelete("delete-by-user-id")]
    public async Task<IActionResult> DeleteWalletByUserId([FromQuery] Guid userId)
    {
        try
        {
            var command = new DeleteWalletByUserIdCommand(userId);
            var result = await _mediator.Send(command);
            if (result.IsError)
                return Problem(result.Errors);
            return Ok("Wallet deleted successfully.");
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while deleting the wallet.");
        }
    }
}